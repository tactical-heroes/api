using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Services;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Move;

public sealed class MoveFileHandler(
    IFilesRepository filesRepository,
    FolderAccessService folderAccessService,
    FileAccessPolicy fileAccessPolicy)
    : ICommandHandler<MoveFileCommand, Result>, IRequireAuthorization, IRequireTransaction
{
    public async Task<Result> HandleAsync(MoveFileCommand command, CancellationToken cancellationToken)
    {
        var id = FileId.Create(command.Id);
        var folderId = FolderId.Create(command.FolderId);
        var validation = Result.Combine(id, folderId);
        if (validation.IsFailure)
        {
            return Result.Failure(validation.Errors);
        }

        var file = await filesRepository.GetByIdAsync(id.Value, cancellationToken);
        if (file is null || !fileAccessPolicy.CanUpdate(file))
        {
            return Result.Failure(Error.NotFound("File was not found."));
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

        var result = file.MoveTo(folder);
        if (result.IsFailure)
        {
            return result;
        }

        filesRepository.Update(file);

        return Result.Success();
    }
}
