using Microsoft.Extensions.Logging;

using PANiXiDA.Core.Application.Storage;
using PANiXiDA.Core.Application.Storage.Models;

using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.ReconcilePersonalUploads;

public sealed class ReconcilePersonalFileUploadsHandler(
    IFilesRepository filesRepository,
    IFileStorage fileStorage,
    IUnitOfWork unitOfWork,
    ILogger<ReconcilePersonalFileUploadsHandler> logger)
    : ICommandHandler<ReconcilePersonalFileUploadsCommand, Result>
{
    public async Task<Result> HandleAsync(
        ReconcilePersonalFileUploadsCommand command,
        CancellationToken cancellationToken)
    {
        await foreach (var file in filesRepository.GetPendingPersonalUploadsAsync(cancellationToken))
        {
            await ReconcileFileAsync(file: file, expireMissing: false, cancellationToken: cancellationToken);
        }

        await foreach (var file in filesRepository.GetExpiredPersonalUploadsAsync(cancellationToken))
        {
            await ReconcileFileAsync(file: file, expireMissing: true, cancellationToken: cancellationToken);
        }

        await foreach (var file in filesRepository.GetPersonalFilesForDeletionAsync(cancellationToken))
        {
            await DeleteFileAsync(file, cancellationToken);
        }

        await foreach (var file in filesRepository.GetDeletedPersonalFilesAsync(cancellationToken))
        {
            await DeleteFileContentAsync(file: file, inspectFirst: true, cancellationToken: cancellationToken);
        }

        return Result.Success();
    }

    private async Task ReconcileFileAsync(
        File file,
        bool expireMissing,
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
            logger.LogError(exception, "Could not inspect personal file {FileId}; will retry", file.Id.Value);
            return;
        }

        if (metadata is null)
        {
            if (expireMissing)
            {
                await BeginDeletionAsync(file, cancellationToken);
            }

            return;
        }

        await CompleteUploadAsync(file: file, metadata: metadata, cancellationToken: cancellationToken);
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
        logger.LogInformation("Expired personal upload {FileId}; will retry deletion until complete", file.Id.Value);
    }

    private async Task CompleteUploadAsync(File file, FileMetadata metadata, CancellationToken cancellationToken)
    {
        var contentType = FileContentType.Create(metadata.ContentType);
        var size = FileSize.Create(metadata.Size);
        if (contentType.IsFailure || size.IsFailure)
        {
            logger.LogWarning("Stored content for personal file {FileId} has invalid metadata", file.Id.Value);
            return;
        }

        var result = file.CompleteUpload(contentType: contentType.Value, size: size.Value);
        if (result.IsSuccess)
        {
            filesRepository.Update(aggregateRoot: file);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Reconciled personal file {FileId}", file.Id.Value);
        }
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
            logger.LogError(exception, "Could not delete personal file {FileId}; will retry", file.Id.Value);
            return false;
        }

        logger.LogInformation("Deleted stored content for personal file {FileId}", file.Id.Value);
        return true;
    }
}
