using PANiXiDA.Core.Application.Messaging.Scheduling;
using PANiXiDA.Core.Application.Storage;
using PANiXiDA.Core.Application.Storage.Constants;
using PANiXiDA.Core.Application.Storage.Models;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.CleanupVersion;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Services;

public sealed class FileRelocationService(
    IFilesRepository filesRepository,
    IFoldersRepository foldersRepository,
    IFolderHierarchyLock hierarchyLock,
    IFileStorage fileStorage,
    IUnitOfWork unitOfWork,
    IScheduler scheduler)
{
    private static readonly TimeSpan s_cleanupDelay = TimeSpan.FromDays(7);

    public async Task<Result> RelocateAsync(File file, CancellationToken cancellationToken)
    {
        if (file.Status != FileStatus.Relocating
            || file.RelocationAttemptId is not { } attemptId
            || file.RelocationStorageKey is not { } destinationKey
            || file.VersionId is not { } sourceVersion
            || file.ContentType is not { } contentType
            || file.Size is not { } size)
        {
            return Result.Failure(Error.Conflict("File relocation has no active content."));
        }

        var metadata = await FindCopyAsync(
            attemptId: attemptId,
            destinationKey: destinationKey,
            contentType: contentType,
            size: size,
            cancellationToken: cancellationToken);
        if (metadata is null)
        {
            var copied = await fileStorage.CopyAsync(
                sourceKey: file.StorageKey.Value,
                sourceVersionId: sourceVersion.Value,
                destinationKey: destinationKey.Value,
                contentType: contentType.Value,
                size: size.Value,
                metadata: new Dictionary<string, string>
                {
                    [FileMetadataNames.OperationAttemptId] = attemptId.Value.ToString()
                },
                cancellationToken: cancellationToken);
            var copiedVersion = FileVersionId.Create(copied.VersionId);
            if (copiedVersion.IsFailure)
            {
                return Result.Failure(Error.Conflict("Copied file content has no storage version."));
            }

            metadata = await fileStorage.GetMetadataAsync(
                key: destinationKey.Value,
                versionId: copiedVersion.Value.Value,
                cancellationToken: cancellationToken);
            if (metadata?.VersionId != copiedVersion.Value.Value)
            {
                return Result.Failure(Error.Conflict("Copied file content has an unexpected storage version."));
            }
        }

        if (!FileRelocationPolicy.CanAccept(
            metadata: metadata,
            attemptId: attemptId,
            contentType: contentType,
            size: size))
        {
            return Result.Failure(Error.Conflict("Copied file content does not match the relocation."));
        }

        return await CompleteAsync(
            file: file,
            attemptId: attemptId,
            destinationKey: destinationKey,
            sourceVersion: sourceVersion,
            versionId: FileVersionId.Create(metadata.VersionId).Value,
            cancellationToken: cancellationToken);
    }

    private async Task<FileMetadata?> FindCopyAsync(
        FileOperationAttemptId attemptId,
        FileStorageKey destinationKey,
        FileContentType contentType,
        FileSize size,
        CancellationToken cancellationToken)
    {
        var current = await fileStorage.GetMetadataAsync(destinationKey.Value, cancellationToken);
        if (FileRelocationPolicy.CanAccept(
            metadata: current,
            attemptId: attemptId,
            contentType: contentType,
            size: size))
        {
            return current;
        }

        var versions = fileStorage.GetVersionsAsync(destinationKey.Value, cancellationToken);
        await foreach (var versionId in versions.Select(version => version.VersionId))
        {
            if (versionId == current?.VersionId)
            {
                continue;
            }

            var metadata = await fileStorage.GetMetadataAsync(
                key: destinationKey.Value,
                versionId: versionId,
                cancellationToken: cancellationToken);
            if (metadata?.VersionId == versionId
                && FileRelocationPolicy.CanAccept(
                    metadata: metadata,
                    attemptId: attemptId,
                    contentType: contentType,
                    size: size))
            {
                return metadata;
            }
        }

        return null;
    }

    private async Task<Result> CompleteAsync(
        File file,
        FileOperationAttemptId attemptId,
        FileStorageKey destinationKey,
        FileVersionId sourceVersion,
        FileVersionId versionId,
        CancellationToken cancellationToken)
    {
        var sourceKey = file.StorageKey;
        var result = Result.Success();
        await unitOfWork.ExecuteInTransactionAsync(
            action: async token =>
            {
                Folder? folder = null;
                if (file.RelocationFolderId is { } folderId)
                {
                    await hierarchyLock.AcquireAsync(token);
                    folder = await foldersRepository.GetUntrackedByIdAsync(folderId, token);
                }

                result = file.CompleteRelocation(
                    attemptId: attemptId,
                    versionId: versionId,
                    folder: folder);
                if (result.IsFailure)
                {
                    var cancelled = file.CancelRelocation(attemptId);
                    if (cancelled.IsFailure)
                    {
                        return;
                    }

                    await ScheduleCleanupAsync(
                        file: file,
                        key: destinationKey,
                        versionId: versionId,
                        cancellationToken: token);
                }
                else
                {
                    await ScheduleCleanupAsync(
                        file: file,
                        key: sourceKey,
                        versionId: sourceVersion,
                        cancellationToken: token);
                }

                filesRepository.Update(file);
                await unitOfWork.SaveChangesAsync(token);
            },
            cancellationToken: cancellationToken);

        return result;
    }

    private Task ScheduleCleanupAsync(
        File file,
        FileStorageKey key,
        FileVersionId versionId,
        CancellationToken cancellationToken)
    {
        return scheduler.ScheduleAsync(
            command: new CleanupFileVersionCommand(
                FileId: file.Id.Value,
                StorageKey: key.Value,
                VersionId: versionId.Value),
            delay: s_cleanupDelay,
            cancellationToken: cancellationToken);
    }
}
