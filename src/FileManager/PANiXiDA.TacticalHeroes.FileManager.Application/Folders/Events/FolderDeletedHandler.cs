using PANiXiDA.Core.Application.Messaging.EventBus;

using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Events;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Events;

public sealed class FolderDeletedHandler(
    IFoldersRepository foldersRepository,
    IEventBus eventBus) : IEventHandler<FolderDeleted>
{
    public async Task HandleAsync(FolderDeleted domainEvent, CancellationToken cancellationToken)
    {
        var folderId = FolderId.Create(domainEvent.FolderId).Value;
        var children = await foldersRepository.GetChildrenBatchAsync(
            parentId: folderId,
            cancellationToken: cancellationToken);
        foreach (var child in children)
        {
            foldersRepository.Delete(child);
            await eventBus.PublishAsync(new FolderDeleted(child.Id.Value), cancellationToken);
        }

        if (children.Count > 0)
        {
            await eventBus.PublishAsync(new FolderDeleted(folderId.Value), cancellationToken);
        }
    }
}
