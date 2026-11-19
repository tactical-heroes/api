using PANiXiDA.TacticalHeroes.FileManager.Application.Files.DeleteStored;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Abstractions;

public interface IFileContentCleanupScheduler
{
    Task ScheduleAsync(FileStorageKey storageKey, CancellationToken cancellationToken);
    Task RescheduleAsync(DeleteStoredFileCommand command, CancellationToken cancellationToken);
}
