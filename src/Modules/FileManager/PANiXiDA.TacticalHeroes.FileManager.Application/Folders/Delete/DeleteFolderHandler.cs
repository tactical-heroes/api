using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.Core.Application.Messaging.EventBus;

using PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Folders.Policies;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Folders.Events;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Folders.Delete;

public sealed class DeleteFolderHandler(
    IFoldersRepository foldersRepository,
    FolderAccessPolicy folderAccessPolicy,
    IEventBus eventBus)
    : ICommandHandler<DeleteFolderCommand, Result>, IRequireAuthorization, IRequireTransaction
{
    public async Task<Result> HandleAsync(DeleteFolderCommand command, CancellationToken cancellationToken)
    {
        var id = FolderId.Create(command.Id);
        if (id.IsFailure)
        {
            return Result.Failure(id.Errors);
        }

        var folder = await foldersRepository.GetByIdAsync(id.Value, cancellationToken);
        if (folder is null || !folderAccessPolicy.CanDelete(folder))
        {
            return Result.Failure(Error.NotFound("Folder was not found."));
        }

        foldersRepository.Delete(folder);
        await eventBus.PublishAsync(new FolderDeleted(folder.Id.Value), cancellationToken);

        return Result.Success();
    }
}
