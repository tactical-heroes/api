using Microsoft.Extensions.Logging;

using PANiXiDA.Core.Application.Storage;
using PANiXiDA.Core.Application.Storage.Models;

using PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.Policies;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.Services;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Files.ValueObjects;

using File = PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.ReconcileUploads;

public sealed class ReconcileFileUploadsHandler(
    IFilesRepository filesRepository,
    IFileStorage fileStorage,
    IUnitOfWork unitOfWork,
    FileRelocationService relocationService,
    FileStorageVersionService fileStorageVersionService,
    ILogger<ReconcileFileUploadsHandler> logger)
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
            await ReconcileContentUpdateAsync(file: file, expireUnusable: false, cancellationToken: cancellationToken);
        }

        await foreach (var file in filesRepository.GetExpiredContentUpdatesAsync(cancellationToken))
        {
            await ReconcileContentUpdateAsync(file: file, expireUnusable: true, cancellationToken: cancellationToken);
        }

        await foreach (var file in filesRepository.GetFilesForDeletionAsync(cancellationToken))
        {
            await DeleteFileAsync(file, cancellationToken);
        }

        await foreach (var file in filesRepository.GetDeletedFilesAsync(cancellationToken))
        {
            await DeleteFileContentAsync(file: file, inspectFirst: true, cancellationToken: cancellationToken);
        }

        await foreach (var file in filesRepository.GetRelocatingFilesAsync(cancellationToken))
        {
            await relocationService.RelocateAsync(file, cancellationToken);
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

    private async Task ReconcileContentUpdateAsync(
        File file,
        bool expireUnusable,
        CancellationToken cancellationToken)
    {
        if (file.OperationAttemptId is not { } contentUpdateAttemptId)
        {
            return;
        }

        FileMetadata? metadata;
        try
        {
            metadata = await fileStorageVersionService.FindAsync(
                key: file.StorageKey,
                canAccept: candidate => FileContentUpdatePolicy.CanAccept(
                    metadata: candidate,
                    activeVersion: file.VersionId,
                    contentUpdateAttemptId: contentUpdateAttemptId),
                excludedVersion: file.VersionId,
                cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not inspect content update for file {FileId}; will retry", file.Id.Value);
            return;
        }

        Result result;
        if (metadata is not null)
        {
            result = file.CompleteContentUpdate(
                contentUpdateAttemptId: contentUpdateAttemptId,
                contentType: FileContentType.Create(metadata.ContentType).Value,
                size: FileSize.Create(metadata.Size).Value,
                versionId: FileVersionId.Create(metadata.VersionId).Value);
        }
        else if (expireUnusable)
        {
            result = file.CancelContentUpdate(contentUpdateAttemptId);
        }
        else
        {
            return;
        }

        if (result.IsSuccess)
        {
            filesRepository.Update(file);
            await unitOfWork.SaveChangesAsync(cancellationToken);
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
        var versionId = FileVersionId.Create(metadata.VersionId);
        if (contentType.IsFailure || size.IsFailure || versionId.IsFailure)
        {
            logger.LogWarning("Stored content for file {FileId} has invalid metadata", file.Id.Value);
            return false;
        }

        var result = file.CompleteUpload(
            contentType: contentType.Value,
            size: size.Value,
            versionId: versionId.Value);
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
