using Microsoft.Extensions.Logging;

using PANiXiDA.Core.Application.Messaging.EventBus;
using PANiXiDA.Core.Application.Storage;
using PANiXiDA.Core.Application.Storage.Models;

using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Events;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.ReconcileUploads;

public sealed class ReconcileFileUploadsHandler(
    IFilesRepository filesRepository,
    IFoldersRepository foldersRepository,
    IEventBus eventBus,
    IFileStorage fileStorage,
    IUnitOfWork unitOfWork,
    ILogger<ReconcileFileUploadsHandler> logger)
    : ICommandHandler<ReconcileFileUploadsCommand, Result>
{
    public async Task<Result> HandleAsync(
        ReconcileFileUploadsCommand command,
        CancellationToken cancellationToken)
    {
        await foreach (var folder in foldersRepository.GetDeletedWithActiveContentsAsync(cancellationToken))
        {
            await unitOfWork.ExecuteInTransactionAsync(
                action: async token =>
                {
                    await eventBus.PublishAsync(new FolderDeleted(folder.Id.Value), token);
                    await unitOfWork.SaveChangesAsync(token);
                },
                cancellationToken: cancellationToken);
        }

        await foreach (var file in filesRepository.GetPendingUploadsAsync(cancellationToken))
        {
            await ReconcileFileAsync(file: file, expireUnusable: false, cancellationToken: cancellationToken);
        }

        await foreach (var file in filesRepository.GetExpiredUploadsAsync(cancellationToken))
        {
            await ReconcileFileAsync(file: file, expireUnusable: true, cancellationToken: cancellationToken);
        }

        await foreach (var file in filesRepository.GetFilesForDeletionAsync(cancellationToken))
        {
            await DeleteFileAsync(file, cancellationToken);
        }

        await foreach (var file in filesRepository.GetDeletedFilesAsync(cancellationToken))
        {
            await DeleteFileContentAsync(file: file, inspectFirst: true, cancellationToken: cancellationToken);
        }

        return Result.Success();
    }

    private async Task ReconcileFileAsync(
        File file,
        bool expireUnusable,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        FileMetadata? metadata;
        try
        {
            metadata = await fileStorage.GetMetadataAsync(file.StorageKey.Value, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not inspect file {FileId}; will retry", file.Id.Value);
            return;
        }

        if (metadata is not null
            && await TryCompleteUploadAsync(file: file, metadata: metadata, cancellationToken: cancellationToken))
        {
            return;
        }

        if (expireUnusable)
        {
            await BeginDeletionAsync(file, cancellationToken);
        }
    }

    private async Task BeginDeletionAsync(File file, CancellationToken cancellationToken)
    {
        if (file.Status != FileStatus.PendingUpload)
        {
            return;
        }

        var result = file.BeginDeletion();
        if (result.IsFailure)
        {
            return;
        }

        filesRepository.Update(aggregateRoot: file);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Expired upload {FileId}; will retry deletion until complete", file.Id.Value);
    }

    private async Task<bool> TryCompleteUploadAsync(
        File file,
        FileMetadata metadata,
        CancellationToken cancellationToken)
    {
        var contentType = FileContentType.Create(metadata.ContentType);
        var size = FileSize.Create(metadata.Size);
        if (contentType.IsFailure || size.IsFailure)
        {
            logger.LogWarning("Stored content for file {FileId} has invalid metadata", file.Id.Value);
            return false;
        }

        var result = file.CompleteUpload(contentType: contentType.Value, size: size.Value);
        if (result.IsSuccess)
        {
            filesRepository.Update(aggregateRoot: file);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Reconciled file {FileId}", file.Id.Value);
        }

        return result.IsSuccess;
    }

    private async Task DeleteFileAsync(File file, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (file.Status != FileStatus.Deleting)
        {
            return;
        }

        if (await DeleteFileContentAsync(file: file, inspectFirst: false, cancellationToken: cancellationToken))
        {
            filesRepository.Delete(aggregateRoot: file);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<bool> DeleteFileContentAsync(File file, bool inspectFirst, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            if (inspectFirst
                && await fileStorage.GetMetadataAsync(file.StorageKey.Value, cancellationToken) is null)
            {
                return true;
            }

            await fileStorage.DeleteAsync(file.StorageKey.Value, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not delete file {FileId}; will retry", file.Id.Value);
            return false;
        }

        logger.LogInformation("Deleted stored content for file {FileId}", file.Id.Value);
        return true;
    }
}
