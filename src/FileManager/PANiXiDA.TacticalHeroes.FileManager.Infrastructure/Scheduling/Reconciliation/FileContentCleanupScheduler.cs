using Microsoft.Extensions.Options;

using PANiXiDA.Core.Application.Messaging.Scheduling;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.DeleteStored;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Scheduling.Options.FileUploadReconciliation;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Scheduling.Reconciliation;

public sealed class FileContentCleanupScheduler(
    IScheduler scheduler,
    IOptions<FileUploadReconciliationOptions> options,
    TimeProvider timeProvider) : IFileContentCleanupScheduler
{
    public Task ScheduleAsync(FileStorageKey storageKey, CancellationToken cancellationToken)
    {
        var command = new DeleteStoredFileCommand(
            StorageKey: storageKey.Value,
            RecheckUntil: timeProvider.GetUtcNow().Add(options.Value.DeletedFileRecheckWindow));

        return scheduler.ScheduleAsync(
            command: command,
            delay: TimeSpan.Zero,
            cancellationToken: cancellationToken);
    }

    public Task RescheduleAsync(DeleteStoredFileCommand command, CancellationToken cancellationToken)
    {
        return timeProvider.GetUtcNow() < command.RecheckUntil
            ? scheduler.ScheduleAsync(
                command: command,
                delay: options.Value.MinimumAge,
                cancellationToken: cancellationToken)
            : Task.CompletedTask;
    }
}
