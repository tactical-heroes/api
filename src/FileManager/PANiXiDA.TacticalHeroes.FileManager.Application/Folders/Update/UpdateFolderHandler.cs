using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Services;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Update;

public sealed class UpdateFolderHandler(
    IFoldersRepository foldersRepository,
    IFoldersReadRepository foldersReadRepository,
    FolderAccessService folderAccessService,
    FolderAccessPolicy folderAccessPolicy)
    : ICommandHandler<UpdateFolderCommand, Result>, IRequireAuthorization, IRequireTransaction
{
    public async Task<Result> HandleAsync(UpdateFolderCommand command, CancellationToken cancellationToken)
    {
        var id = FolderId.Create(command.Id);
        var name = FolderName.Create(command.Name);
        var type = FileType.Create(command.AllowedFileType);
        var ownerId = UserId.Create(command.UserId);
        var parentId = FolderId.Create(command.ParentId);
        var validation = Result.Combine(id, name, type, ownerId, parentId);
        if (validation.IsFailure)
        {
            return Result.Failure(validation.Errors);
        }

        var source = await folderAccessService.GetForUpdateAsync(id.Value, cancellationToken);
        if (source.IsFailure)
        {
            return Result.Failure(source.Errors);
        }

        var folder = source.Value;

        if (!folderAccessPolicy.CanUpdate(type.Value, ownerId.Value))
        {
            return Result.Failure(Error.Forbidden("Access to the requested folder type or owner is denied."));
        }

        Folder? parent = null;
        if (parentId.Value is { } targetId)
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

        var changesScope = type.Value != folder.AllowedFileType || ownerId.Value != folder.UserId;
        if (changesScope && await foldersReadRepository.HasContentsAsync(folder.Id.Value, cancellationToken))
        {
            return Result.Failure(
                Error.Conflict("Folder type and owner can only be changed when the folder is empty."));
        }

        var result = folder.Update(
            name: name.Value,
            allowedFileType: type.Value,
            userId: ownerId.Value,
            parent: parent);
        if (result.IsFailure)
        {
            return result;
        }

        foldersRepository.Update(folder);

        return Result.Success();
    }
}
