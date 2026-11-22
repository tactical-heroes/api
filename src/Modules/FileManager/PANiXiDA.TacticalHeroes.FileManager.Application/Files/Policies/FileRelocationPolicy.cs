using System.Diagnostics.CodeAnalysis;

using PANiXiDA.Core.Application.Storage.Constants;
using PANiXiDA.Core.Application.Storage.Models;

using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Files.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.Policies;

public static class FileRelocationPolicy
{
    public const long MaximumCopySize = 5L * 1024 * 1024 * 1024;

    public static bool CanAccept(
        [NotNullWhen(true)] FileMetadata? metadata,
        FileOperationAttemptId attemptId,
        FileContentType contentType,
        FileSize size)
    {
        return metadata is not null
            && metadata.ContentType == contentType.Value
            && metadata.Size == size.Value
            && FileVersionId.Create(metadata.VersionId).IsSuccess
            && metadata.Metadata.TryGetValue(FileMetadataNames.OperationAttemptId, out var storedAttempt)
            && Guid.TryParse(storedAttempt, out var parsedAttempt)
            && parsedAttempt == attemptId.Value;
    }
}
