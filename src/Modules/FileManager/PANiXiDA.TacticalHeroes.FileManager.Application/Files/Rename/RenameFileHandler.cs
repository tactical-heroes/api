using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Rename;

public sealed class RenameFileHandler(IFilesRepository filesRepository, FileAccessPolicy fileAccessPolicy)
    : ICommandHandler<RenameFileCommand, Result>, IRequireAuthorization, IRequireTransaction
{
    public async Task<Result> HandleAsync(RenameFileCommand command, CancellationToken cancellationToken)
    {
        var id = FileId.Create(command.Id);
        if (id.IsFailure)
        {
            return Result.Failure(id.Errors);
        }

        var file = await filesRepository.GetByIdAsync(id.Value, cancellationToken);
        if (file is null || !fileAccessPolicy.CanUpdate(file))
        {
            return Result.Failure(Error.NotFound("File was not found."));
        }

        var name = FileName.Create(command.Name);
        if (name.IsFailure)
        {
            return Result.Failure(name.Errors);
        }

        var result = file.Rename(name.Value);
        if (result.IsSuccess)
        {
            filesRepository.Update(file);
        }

        return result;
    }
}
