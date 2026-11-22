using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Services;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Folders.CreatePersonal;

public sealed class CreatePersonalFolderHandler(FolderCreationService folderCreationService, ICurrentUser currentUser)
    : ICommandHandler<CreatePersonalFolderCommand, Result<Guid>>, IRequireAuthorization, IRequireTransaction
{
    public Task<Result<Guid>> HandleAsync(
        CreatePersonalFolderCommand command,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Task.FromResult(Result.Failure<Guid>(Error.Unauthorized("A user account is required.")));
        }

        return folderCreationService.CreateAsync(
            name: command.Name,
            allowedFileType: FileType.Personal,
            userId: userId,
            parentId: command.ParentId,
            cancellationToken: cancellationToken);
    }
}
