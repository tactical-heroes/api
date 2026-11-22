using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Rename;

public sealed class RenameFolderHandler(IFoldersRepository foldersRepository, FolderAccessPolicy folderAccessPolicy)
    : ICommandHandler<RenameFolderCommand, Result>, IRequireAuthorization, IRequireTransaction
{
    public async Task<Result> HandleAsync(RenameFolderCommand command, CancellationToken cancellationToken)
    {
        var id = FolderId.Create(command.Id);
        if (id.IsFailure)
        {
            return Result.Failure(id.Errors);
        }

        var folder = await foldersRepository.GetByIdAsync(id.Value, cancellationToken);
        if (folder is null || !folderAccessPolicy.CanUpdate(folder))
        {
            return Result.Failure(Error.NotFound("Folder was not found."));
        }

        var name = FolderName.Create(command.Name);
        if (name.IsFailure)
        {
            return Result.Failure(name.Errors);
        }

        folder.Rename(name.Value);
        foldersRepository.Update(folder);

        return Result.Success();
    }
}
