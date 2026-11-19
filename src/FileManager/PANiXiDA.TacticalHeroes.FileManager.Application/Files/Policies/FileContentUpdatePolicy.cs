using System.Diagnostics.CodeAnalysis;

using PANiXiDA.Core.Application.Storage.Models;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Common;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;

internal static class FileContentUpdatePolicy
{
    public static bool CanAccept(
        [NotNullWhen(true)] FileMetadata? metadata,
        FileVersionId? activeVersion,
        FileContentUpdateAttemptId contentUpdateAttemptId)
    {
        return metadata is not null
            && metadata.VersionId != activeVersion?.Value
            && metadata.Metadata.TryGetValue(FileMetadataNames.ContentUpdateAttemptId, out var storedUpdateId)
            && Guid.TryParse(storedUpdateId, out var storedId)
            && storedId == contentUpdateAttemptId.Value
            && FileContentType.Create(metadata.ContentType).IsSuccess
            && FileSize.Create(metadata.Size).IsSuccess
            && FileVersionId.Create(metadata.VersionId).IsSuccess;
    }
}
