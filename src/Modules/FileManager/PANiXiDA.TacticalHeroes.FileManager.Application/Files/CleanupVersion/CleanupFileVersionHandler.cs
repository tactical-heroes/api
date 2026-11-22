using PANiXiDA.Core.Application.Storage;

using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Files;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Files.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.CleanupVersion;

public sealed class CleanupFileVersionHandler(IFilesRepository filesRepository, IFileStorage fileStorage)
    : ICommandHandler<CleanupFileVersionCommand, Result>
{
    public async Task<Result> HandleAsync(CleanupFileVersionCommand command, CancellationToken cancellationToken)
    {
        var id = FileId.Create(command.FileId);
        var key = FileStorageKey.Create(command.StorageKey);
        var version = FileVersionId.Create(command.VersionId);
        var validation = Result.Combine(id, key, version);
        if (validation.IsFailure)
        {
            return Result.Failure(validation.Errors);
        }

        var file = await filesRepository.GetByIdAsync(id.Value, cancellationToken);
        var activeVersionId = file is not null && file.StorageKey == key.Value ? file.VersionId?.Value : null;
        if (activeVersionId == version.Value.Value)
        {
            return Result.Success();
        }

        var versions = await fileStorage.GetVersionsAsync(key.Value.Value, cancellationToken)
            .ToListAsync(cancellationToken);
        var boundaryIndex = versions.FindIndex(storedVersion => storedVersion.VersionId == version.Value.Value);
        if (boundaryIndex < 0)
        {
            return Result.Success();
        }

        var retiredVersionIds = versions
            .Skip(boundaryIndex + 1)
            .Append(versions[boundaryIndex])
            .Select(retiredVersion => retiredVersion.VersionId)
            .Where(retiredVersionId => retiredVersionId != activeVersionId);
        foreach (var retiredVersionId in retiredVersionIds)
        {
            await fileStorage.DeleteAsync(
                key: key.Value.Value,
                versionId: retiredVersionId,
                cancellationToken: cancellationToken);
        }

        return Result.Success();
    }
}
