using PANiXiDA.Core.Application.Storage;

using PANiXiDA.TacticalHeroes.FileManager.Domain.Files;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.CleanupVersion;

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
        if (file is not null && file.StorageKey == key.Value && file.VersionId == version.Value)
        {
            return Result.Success();
        }

        await fileStorage.DeleteAsync(
            key: key.Value.Value,
            versionId: version.Value.Value,
            cancellationToken: cancellationToken);

        return Result.Success();
    }
}
