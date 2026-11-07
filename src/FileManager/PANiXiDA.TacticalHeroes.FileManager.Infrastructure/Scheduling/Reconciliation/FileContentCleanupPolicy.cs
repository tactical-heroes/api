using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Entities.FileContents;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Scheduling.Options.FileUploadReconciliation;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Scheduling.Reconciliation;

public sealed class FileContentCleanupPolicy(
    IOptions<FileUploadReconciliationOptions> options,
    TimeProvider timeProvider)
    : IFileContentCleanupPolicy
{
    public bool CanBeginDeletion(FileContent content)
    {
        return content.DeletionStartedAt is not null
            || content.RegisteredAt.Value < timeProvider.GetUtcNow().Subtract(options.Value.MinimumAge);
    }

    public bool CanForget(FileContent content)
    {
        return content.DeletionStartedAt is { } startedAt
            && startedAt.Value < timeProvider.GetUtcNow().Subtract(options.Value.DeletedFileRecheckWindow);
    }
}
