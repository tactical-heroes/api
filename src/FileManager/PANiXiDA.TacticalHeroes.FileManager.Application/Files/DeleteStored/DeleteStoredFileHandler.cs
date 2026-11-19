using PANiXiDA.Core.Application.Storage;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.DeleteStored;

public sealed class DeleteStoredFileHandler(
    IFileStorage fileStorage,
    IFileContentCleanupScheduler cleanupScheduler)
    : ICommandHandler<DeleteStoredFileCommand, Result>, IRequireTransaction
{
    public async Task<Result> HandleAsync(DeleteStoredFileCommand command, CancellationToken cancellationToken)
    {
        if (await fileStorage.GetMetadataAsync(command.StorageKey, cancellationToken) is not null)
        {
            await fileStorage.DeleteAsync(command.StorageKey, cancellationToken);
        }
        await cleanupScheduler.RescheduleAsync(command, cancellationToken);

        return Result.Success();
    }
}
