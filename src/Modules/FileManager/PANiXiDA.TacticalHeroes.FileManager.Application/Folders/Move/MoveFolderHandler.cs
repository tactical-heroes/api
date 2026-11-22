using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Services;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Move;

public sealed class MoveFolderHandler(
    IFoldersRepository foldersRepository,
    IFoldersReadRepository foldersReadRepository,
    FolderAccessService folderAccessService)
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

        var source = await folderAccessService.GetForUpdateAsync(id.Value, cancellationToken);
        if (source.IsFailure)
        {
            return Result.Failure(source.Errors);
        }

        var folder = source.Value;

        Folder? parent = null;
        if (parentId is { } targetId)
        {
            var destination = await folderAccessService.GetForUpdateAsync(targetId, cancellationToken);
            if (destination.IsFailure)
            {
                return Result.Failure(destination.Errors);
            }

            parent = destination.Value;

            if (await foldersReadRepository.HasAncestorAsync(
                id: targetId.Value,
                ancestorId: folder.Id.Value,
                cancellationToken: cancellationToken))
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
