using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Services;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Create;

public sealed class CreateFolderHandler(FolderCreationService folderCreationService)
    : ICommandHandler<CreateFolderCommand, Result<Guid>>, IRequireAuthorization, IRequireTransaction
{
    public Task<Result<Guid>> HandleAsync(CreateFolderCommand command, CancellationToken cancellationToken)
    {
        var type = FileType.Create(command.AllowedFileType);
        if (type.IsFailure)
        {
            return Task.FromResult(Result.Failure<Guid>(type.Errors));
        }

        return folderCreationService.CreateAsync(
            name: command.Name,
            allowedFileType: type.Value,
            userId: command.UserId,
            parentId: command.ParentId,
            cancellationToken: cancellationToken);
    }
}
