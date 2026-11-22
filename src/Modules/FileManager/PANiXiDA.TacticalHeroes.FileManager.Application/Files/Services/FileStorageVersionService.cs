using PANiXiDA.Core.Application.Storage;
using PANiXiDA.Core.Application.Storage.Models;

using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Services;

public sealed class FileStorageVersionService(IFileStorage fileStorage)
{
    public async Task<FileMetadata?> FindAsync(
        FileStorageKey key,
        Func<FileMetadata, bool> canAccept,
        FileVersionId? excludedVersion,
        CancellationToken cancellationToken)
    {
        var current = await fileStorage.GetMetadataAsync(key.Value, cancellationToken);
        if (current is not null && current.VersionId != excludedVersion?.Value && canAccept(current))
        {
            return current;
        }

        var versions = fileStorage.GetVersionsAsync(key.Value, cancellationToken);
        await foreach (var versionId in versions.Select(version => version.VersionId))
        {
            if (versionId == current?.VersionId || versionId == excludedVersion?.Value)
            {
                continue;
            }

            var metadata = await fileStorage.GetMetadataAsync(
                key: key.Value,
                versionId: versionId,
                cancellationToken: cancellationToken);
            if (metadata is not null && metadata.VersionId == versionId && canAccept(metadata))
            {
                return metadata;
            }
        }

        return null;
    }
}
