using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.Core.Application.Storage;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.CompleteUpload;

public sealed class CompleteFileUploadHandler(
    IFilesRepository filesRepository,
    IFileStorage fileStorage,
    FileAccessPolicy fileAccessPolicy)
    : ICommandHandler<CompleteFileUploadCommand, Result>, IRequireAuthorization
{
    public async Task<Result> HandleAsync(CompleteFileUploadCommand command, CancellationToken cancellationToken)
    {
        var id = FileId.Create(command.Id);
        if (id.IsFailure)
        {
            return Result.Failure(id.Errors);
        }

        var file = await filesRepository.GetByIdAsync(id.Value, cancellationToken);
        if (file is null || !fileAccessPolicy.CanCreate(file))
        {
            return Result.Failure(Error.NotFound("File was not found."));
        }

        var metadata = await fileStorage.GetMetadataAsync(file.StorageKey.Value, cancellationToken);
        if (metadata is null)
        {
            return Result.Failure(Error.Conflict("File content has not been uploaded."));
        }

        var contentType = FileContentType.Create(metadata.ContentType);
        var size = FileSize.Create(metadata.Size);
        if (contentType.IsFailure || size.IsFailure)
        {
            return Result.Failure(Error.Conflict("Uploaded file metadata is invalid."));
        }

        var previousStatus = file.Status;
        var result = file.CompleteUpload(contentType: contentType.Value, size: size.Value);
        if (result.IsSuccess && previousStatus != file.Status)
        {
            filesRepository.Update(file);
        }

        return result;
    }
}
