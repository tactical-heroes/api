using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Services;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Update;

public sealed class UpdateFileHandler(
    IFilesRepository filesRepository,
    FolderAccessService folderAccessService,
    FileAccessPolicy fileAccessPolicy)
    : ICommandHandler<UpdateFileCommand, Result>, IRequireAuthorization, IRequireTransaction
{
    public async Task<Result> HandleAsync(UpdateFileCommand command, CancellationToken cancellationToken)
    {
        var id = FileId.Create(command.Id);
        var name = FileName.Create(command.Name);
        var type = FileType.Create(command.Type);
        var ownerId = UserId.Create(command.UserId);
        var folderId = FolderId.Create(command.FolderId);
        var validation = Result.Combine(id, name, type, ownerId, folderId);
        if (validation.IsFailure)
        {
            return Result.Failure(validation.Errors);
        }

        var file = await filesRepository.GetByIdAsync(id.Value, cancellationToken);
        if (file is null || !fileAccessPolicy.CanUpdate(file))
        {
            return Result.Failure(Error.NotFound("File was not found."));
        }

        if (!fileAccessPolicy.CanUpdate(type.Value, ownerId.Value))
        {
            return Result.Failure(Error.Forbidden("Access to the requested file type or owner is denied."));
        }

        Folder? folder = null;
        if (folderId.Value is { } targetId)
        {
            var destination = await folderAccessService.GetForUpdateAsync(targetId, cancellationToken);
            if (destination.IsFailure)
            {
                return Result.Failure(destination.Errors);
            }

            folder = destination.Value;
        }

        var result = file.Update(name: name.Value, type: type.Value, userId: ownerId.Value, folder: folder);
        if (result.IsFailure)
        {
            return result;
        }

        filesRepository.Update(file);

        return Result.Success();
    }
}
