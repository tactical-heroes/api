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
    FolderAccessPolicy folderAccessPolicy)
    : ICommandHandler<CreateFolderCommand, Result<Guid>>, IRequireAuthorization, IRequireTransaction
{
    public async Task<Result<Guid>> HandleAsync(CreateFolderCommand command, CancellationToken cancellationToken)
    {
        var result = command.ParentId is not null
            ? await CreateChildAsync(command, cancellationToken)
            : CreateRoot(command);
        if (result.IsFailure)
        {
            return Result.Failure<Guid>(result.Errors);
        }

        foldersRepository.Add(result.Value);

        return Result.Success(result.Value.Id.Value);
    }

    private Result<Folder> CreateRoot(CreateFolderCommand command)
    {
        var name = FolderName.Create(command.Name);
        var type = FileType.Create(command.AllowedFileType);
        var userId = UserId.Create(command.UserId);
        var validation = Result.Combine(name, type, userId);
        if (validation.IsFailure)
        {
            return Result.Failure<Folder>(validation.Errors);
        }

        var result = Folder.Create(
            name: name.Value,
            allowedFileType: type.Value,
            userId: userId.Value);
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
        CancellationToken cancellationToken)
    {
        var name = FolderName.Create(command.Name);
        var type = FileType.Create(command.AllowedFileType);
        var parentId = FolderId.Create(command.ParentId.GetValueOrDefault());
        var validation = Result.Combine(name, type, parentId);
        if (validation.IsFailure)
        {
            return Result.Failure<Folder>(validation.Errors);
        }

        var parent = await foldersRepository.GetByIdAsync(parentId.Value, cancellationToken);
        if (parent is null || !folderAccessPolicy.CanCreate(parent))
        {
            return Result.Failure<Folder>(Error.NotFound("Folder was not found."));
        }

        if (type.Value != parent.AllowedFileType)
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

        return Result.Success(parent.CreateChild(name.Value));
    }
}
