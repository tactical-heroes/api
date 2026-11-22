using PANiXiDA.Core.Application.Messaging.EventBus;

using PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Folders.Events;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Folders.ReconcileDeletions;

public sealed class ReconcileFolderDeletionsHandler(
    IFoldersReadRepository foldersReadRepository,
    IEventBus eventBus)
    : ICommandHandler<ReconcileFolderDeletionsCommand, Result>, IRequireTransaction
{
    public async Task<Result> HandleAsync(
        ReconcileFolderDeletionsCommand command,
        CancellationToken cancellationToken)
    {
        var folderIds = await foldersReadRepository.GetDeletedIdsWithActiveContentsAsync(cancellationToken);
        foreach (var folderId in folderIds)
        {
            await eventBus.PublishAsync(new FolderDeleted(folderId), cancellationToken);
        }

        return Result.Success();
    }
}
