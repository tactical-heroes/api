using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Delete;

public sealed class DeleteFileHandler(IFilesRepository filesRepository, FileAccessPolicy fileAccessPolicy)
    : ICommandHandler<DeleteFileCommand, Result>, IRequireAuthorization, IRequireTransaction
{
    public async Task<Result> HandleAsync(DeleteFileCommand command, CancellationToken cancellationToken)
    {
        var fileId = FileId.Create(command.Id);
        if (fileId.IsFailure)
        {
            return Result.Failure(fileId.Errors);
        }

        var file = await filesRepository.GetByIdAsync(fileId.Value, cancellationToken);
        if (file is null || !fileAccessPolicy.CanDelete(file))
        {
            return Result.Failure(Error.NotFound("File was not found."));
        }

        var result = file.BeginDeletion();
        if (result.IsSuccess)
        {
            filesRepository.Update(file);
        }

        return result;
    }
}
