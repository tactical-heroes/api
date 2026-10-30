using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Services;

public sealed class FolderCreationService(IFoldersRepository foldersRepository, FolderAccessPolicy folderAccessPolicy)
{
    public async Task<Result<Guid>> CreateAsync(
        string name,
        FileType allowedFileType,
        Guid? userId,
        Guid? parentId,
        CancellationToken cancellationToken)
    {
        var folderName = FolderName.Create(name);
        var ownerId = UserId.Create(userId);
        var validation = Result.Combine(folderName, ownerId);
        if (validation.IsFailure)
        {
            return Result.Failure<Guid>(validation.Errors);
        }

        Result<Folder> result;
        if (parentId is { } value)
        {
            var parentIdResult = FolderId.Create(value);
            if (parentIdResult.IsFailure)
            {
                return Result.Failure<Guid>(parentIdResult.Errors);
            }

            result = await CreateChildAsync(
                name: folderName.Value,
                allowedFileType: allowedFileType,
                userId: ownerId.Value,
                parentId: parentIdResult.Value,
                cancellationToken: cancellationToken);
        }
        else
        {
            result = CreateRoot(
                name: folderName.Value,
                allowedFileType: allowedFileType,
                userId: ownerId.Value);
        }

        if (result.IsFailure)
        {
            return Result.Failure<Guid>(result.Errors);
        }

        foldersRepository.Add(result.Value);

        return Result.Success(result.Value.Id.Value);
    }

    private Result<Folder> CreateRoot(FolderName name, FileType allowedFileType, UserId? userId)
    {
        var result = Folder.Create(
            name: name,
            allowedFileType: allowedFileType,
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
        FolderName name,
        FileType allowedFileType,
        UserId? userId,
        FolderId parentId,
        CancellationToken cancellationToken)
    {
        var parent = await foldersRepository.GetByIdAsync(parentId, cancellationToken);
        if (parent is null
            || !folderAccessPolicy.CanCreate(parent)
            || allowedFileType != parent.AllowedFileType
            || (userId is { } ownerId && ownerId != parent.UserId))
        {
            return Result.Failure<Folder>(Error.NotFound("Folder was not found."));
        }

        return Result.Success(parent.CreateChild(name));
    }
}
