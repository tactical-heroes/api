using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Move;

public sealed class MoveFolderHandler(
    IFoldersRepository foldersRepository,
    IFoldersReadRepository foldersReadRepository,
    FolderAccessPolicy folderAccessPolicy)
    : ICommandHandler<MoveFolderCommand, Result>, IRequireAuthorization, IRequireTransaction
{
    public async Task<Result> HandleAsync(MoveFolderCommand command, CancellationToken cancellationToken)
    {
        var id = FolderId.Create(command.Id);
        if (id.IsFailure)
        {
            return Result.Failure(id.Errors);
        }

        FolderId? parentId = null;
        if (command.ParentId is { } value)
        {
            var result = FolderId.Create(value);
            if (result.IsFailure)
            {
                return Result.Failure(result.Errors);
            }

            parentId = result.Value;
        }

        await foldersRepository.LockHierarchyAsync(cancellationToken);
        var folder = await foldersRepository.GetByIdAsync(id.Value, cancellationToken);
        if (folder is null || !folderAccessPolicy.CanUpdate(folder))
        {
            return Result.Failure(Error.NotFound("Folder was not found."));
        }

        Folder? parent = null;
        if (parentId is { } targetId)
        {
            parent = await foldersRepository.GetByIdAsync(targetId, cancellationToken);
            if (parent is null || !folderAccessPolicy.CanUpdate(parent))
            {
                return Result.Failure(Error.NotFound("Folder was not found."));
            }

            var ancestorIds = await foldersReadRepository.GetAncestorIdsAsync(targetId.Value, cancellationToken);
            if (ancestorIds.Contains(folder.Id.Value))
            {
                return Result.Failure(
                    Error.Validation("A folder cannot be moved into one of its descendants.")
                        .WithField(nameof(Folder.ParentId)));
            }
        }

        var moveResult = folder.MoveTo(parent);
        if (moveResult.IsFailure)
        {
            return moveResult;
        }

        foldersRepository.Update(folder);

        return Result.Success();
    }
}
