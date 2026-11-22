using PANiXiDA.Core.Application.Messaging.Scheduling;
using PANiXiDA.Core.Application.Storage;
using PANiXiDA.Core.Application.Storage.Constants;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.CleanupVersion;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Services;

public sealed class FileRelocationService(
    IFilesRepository filesRepository,
    IFileStorage fileStorage,
    FileStorageVersionService fileStorageVersionService,
    IUnitOfWork unitOfWork,
    IScheduler scheduler)
{
    private static readonly TimeSpan s_cleanupDelay = TimeSpan.FromDays(7);

    public async Task<Result> RelocateAsync(File file, CancellationToken cancellationToken)
    {
        if (file.Status != FileStatus.Relocating
            || file.OperationAttemptId is not { } attemptId
            || file.VersionId is not { } sourceVersion
            || file.ContentType is not { } contentType
            || file.Size is not { } size)
        {
            return Result.Failure(Error.Conflict("File relocation has no active content."));
        }

        var destinationKey = FileStorageKey.Create(id: file.Id, type: file.Type, userId: file.UserId).Value;
        var metadata = await fileStorageVersionService.FindAsync(
            key: destinationKey,
            canAccept: candidate => FileRelocationPolicy.CanAccept(
                metadata: candidate,
                attemptId: attemptId,
                contentType: contentType,
                size: size),
            excludedVersion: null,
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
            sourceVersion: sourceVersion,
            versionId: FileVersionId.Create(metadata.VersionId).Value,
            cancellationToken: cancellationToken);
    }

    private async Task<Result> CompleteAsync(
        File file,
        FileOperationAttemptId attemptId,
        FileVersionId sourceVersion,
        FileVersionId versionId,
        CancellationToken cancellationToken)
    {
        var sourceKey = file.StorageKey;
        var result = Result.Success();
        await unitOfWork.ExecuteInTransactionAsync(
            action: async token =>
            {
                result = file.CompleteRelocation(attemptId, versionId);
                if (result.IsFailure)
                {
                    return;
                }

                filesRepository.Update(file);
                await scheduler.ScheduleAsync(
                    command: new CleanupFileVersionCommand(
                        FileId: file.Id.Value,
                        StorageKey: sourceKey.Value,
                        VersionId: sourceVersion.Value),
                    delay: s_cleanupDelay,
                    cancellationToken: token);
                await unitOfWork.SaveChangesAsync(token);
            },
            cancellationToken: cancellationToken);

        return result;
    }
}
