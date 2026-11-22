using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Services;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Create;

public sealed class CreateFolderHandler(FolderCreationService folderCreationService)
    : ICommandHandler<CreateFolderCommand, Result<Guid>>, IRequireAuthorization, IRequireTransaction
{
    public Task<Result<Guid>> HandleAsync(CreateFolderCommand command, CancellationToken cancellationToken)
    {
        return folderCreationService.CreateAsync(
            name: command.Name,
            allowedFileType: FileType.Create(command.AllowedFileType).Value,
            userId: command.UserId,
            parentId: command.ParentId,
            cancellationToken: cancellationToken);
    }
}
