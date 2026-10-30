using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Create;

public sealed class CreateFolderHandler(
    IFoldersRepository foldersRepository,
    FolderAccessPolicy folderAccessPolicy,
    ICurrentUser currentUser)
    : ICommandHandler<CreateFolderCommand, Result<Guid>>, IRequireAuthorization, IRequireTransaction
{
    public async Task<Result<Guid>> HandleAsync(CreateFolderCommand command, CancellationToken cancellationToken)
    {
        var name = FolderName.Create(command.Name);
        var type = FileType.Create(command.AllowedFileType);
        var validation = Result.Combine(name, type);
        if (validation.IsFailure)
        {
            return Result.Failure<Guid>(validation.Errors);
        }

        var result = command.ParentId is { } parentId
            ? await CreateChildAsync(
                command: command,
                name: name.Value,
                type: type.Value,
                parentId: parentId,
                cancellationToken: cancellationToken)
            : CreateRoot(
                command: command,
                name: name.Value,
                type: type.Value);
        if (result.IsFailure)
        {
            return Result.Failure<Guid>(result.Errors);
        }

        foldersRepository.Add(result.Value);

        return Result.Success(result.Value.Id.Value);
    }

    private Result<Folder> CreateRoot(CreateFolderCommand command, FolderName name, FileType type)
    {
        var ownerId = command.UserId ?? (type == FileType.Personal ? currentUser.UserId : null);
        UserId? userId = null;
        if (ownerId is { } id)
        {
            var owner = UserId.Create(id);
            if (owner.IsFailure)
            {
                return Result.Failure<Folder>(owner.Errors);
            }

            userId = owner.Value;
        }

        var result = Folder.Create(
            name: name,
            allowedFileType: type,
            userId: userId);
        if (result.IsFailure)
        {
            return result;
        }

        return folderAccessPolicy.CanCreate(result.Value)
            ? result
            : Result.Failure<Folder>(Error.Forbidden("Access to folders is denied."));
    }

    private async Task<Result<Folder>> CreateChildAsync(
        CreateFolderCommand command,
        FolderName name,
        FileType type,
        Guid parentId,
        CancellationToken cancellationToken)
    {
        var id = FolderId.Create(parentId);
        if (id.IsFailure)
        {
            return Result.Failure<Folder>(id.Errors);
        }

        var parent = await foldersRepository.GetByIdAsync(id.Value, cancellationToken);
        if (parent is null || !folderAccessPolicy.CanCreate(parent))
        {
            return Result.Failure<Folder>(Error.NotFound("Folder was not found."));
        }

        if (type != parent.AllowedFileType)
        {
            return Result.Failure<Folder>(
                Error.Validation("Folder type must match the parent's allowed file type.")
                    .WithField(nameof(command.AllowedFileType)));
        }

        if (command.UserId is { } ownerId && ownerId != parent.UserId?.Value)
        {
            return Result.Failure<Folder>(
                Error.Validation("Folder user id must match the parent's user id.")
                    .WithField(nameof(command.UserId)));
        }

        return Result.Success(parent.CreateChild(name));
    }
}
