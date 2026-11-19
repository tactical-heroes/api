using Microsoft.Extensions.Logging;

using PANiXiDA.Core.Application.Storage;
using PANiXiDA.Core.Application.Storage.Models;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.ReconcileUploads;

public sealed class ReconcileFileUploadsHandler(
    IFilesRepository filesRepository,
    IFileStorage fileStorage,
    IUnitOfWork unitOfWork,
    ILogger<ReconcileFileUploadsHandler> logger,
    IFileContentCleanupScheduler cleanupScheduler)
    : ICommandHandler<ReconcileFileUploadsCommand, Result>
{
    public async Task<Result> HandleAsync(
        ReconcileFileUploadsCommand command,
        CancellationToken cancellationToken)
    {
        await foreach (var file in filesRepository.GetPendingUploadsAsync(cancellationToken))
        {
            await ReconcileFileAsync(file: file, expireUnusable: false, cancellationToken: cancellationToken);
        }

        await foreach (var file in filesRepository.GetExpiredUploadsAsync(cancellationToken))
        {
            await ReconcileFileAsync(file: file, expireUnusable: true, cancellationToken: cancellationToken);
        }

        await foreach (var file in filesRepository.GetUpdatingFilesAsync(cancellationToken))
        {
            await ReconcileFileAsync(file: file, expireUnusable: false, cancellationToken: cancellationToken);
        }

        await foreach (var file in filesRepository.GetExpiredContentUpdatesAsync(cancellationToken))
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
        var pendingKey = file.PendingStorageKey;
        var storageKey = pendingKey ?? file.StorageKey;
        FileMetadata? metadata;
        try
        {
            metadata = await fileStorage.GetMetadataAsync(storageKey.Value, cancellationToken);
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
            && await TryCompleteUploadAsync(
                file: file,
                metadata: metadata,
                pendingKey: pendingKey,
                cancellationToken: cancellationToken))
        {
            return;
        }

        if (expireUnusable)
        {
            if (pendingKey is not null)
            {
                var cancelled = file.CancelContentUpdate(pendingKey);
                if (cancelled.IsSuccess)
                {
                    await SaveContentChangeAsync(
                        file: file,
                        unusedKey: cancelled.Value,
                        cancellationToken: cancellationToken);
                }
            }
            else
            {
                await BeginDeletionAsync(file, cancellationToken);
            }
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
        FileStorageKey? pendingKey,
        CancellationToken cancellationToken)
    {
        var contentType = FileContentType.Create(metadata.ContentType);
        var size = FileSize.Create(metadata.Size);
        if (contentType.IsFailure || size.IsFailure)
        {
            logger.LogWarning("Stored content for file {FileId} has invalid metadata", file.Id.Value);
            return false;
        }

        if (pendingKey is not null)
        {
            var completed = file.CompleteContentUpdate(
                pendingStorageKey: pendingKey,
                contentType: contentType.Value,
                size: size.Value);
            if (completed.IsFailure)
            {
                return false;
            }

            await SaveContentChangeAsync(
                file: file,
                unusedKey: completed.Value,
                cancellationToken: cancellationToken);

            return true;
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

    private Task SaveContentChangeAsync(File file, FileStorageKey unusedKey, CancellationToken cancellationToken)
    {
        return unitOfWork.ExecuteInTransactionAsync(
            action: async token =>
            {
                filesRepository.Update(file);
                await cleanupScheduler.ScheduleAsync(unusedKey, token);
                await unitOfWork.SaveChangesAsync(token);
            },
            cancellationToken: cancellationToken);
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
