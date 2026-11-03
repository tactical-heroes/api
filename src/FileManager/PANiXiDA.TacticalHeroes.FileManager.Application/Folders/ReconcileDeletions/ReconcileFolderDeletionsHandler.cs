using PANiXiDA.Core.Application.Messaging.EventBus;

using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Events;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Folders.ReconcileDeletions;

public sealed class ReconcileFolderDeletionsHandler(IFoldersRepository foldersRepository, IEventBus eventBus)
    : ICommandHandler<ReconcileFolderDeletionsCommand, Result>, IRequireTransaction
{
    public async Task<Result> HandleAsync(
        ReconcileFolderDeletionsCommand command,
        CancellationToken cancellationToken)
    {
        await foreach (var folder in foldersRepository.GetDeletedWithActiveContentsAsync(cancellationToken))
        {
            await eventBus.PublishAsync(new FolderDeleted(folder.Id.Value), cancellationToken);
        }

        return Result.Success();
    }
}
