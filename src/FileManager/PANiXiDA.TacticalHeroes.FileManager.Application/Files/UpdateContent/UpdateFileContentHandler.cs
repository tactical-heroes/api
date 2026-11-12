using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.Core.Application.Storage;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Entities.FileContents.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.UpdateContent;

public sealed class UpdateFileContentHandler(
    IFilesRepository filesRepository,
    IFileStorage fileStorage,
    IUnitOfWork unitOfWork,
    FileAccessPolicy fileAccessPolicy,
    TimeProvider timeProvider)
    : ICommandHandler<UpdateFileContentCommand, Result>, IRequireAuthorization
{
    public async Task<Result> HandleAsync(UpdateFileContentCommand command, CancellationToken cancellationToken)
    {
        var id = FileId.Create(command.Id);
        var contentType = FileContentType.Create(command.ContentType);
        var size = FileSize.Create(command.Size);
        var validation = Result.Combine(id, contentType, size);
        if (validation.IsFailure)
        {
            return Result.Failure(validation.Errors);
        }

        if (command.Content is not { CanRead: true })
        {
            return Result.Failure(Error.Validation("File content must be readable."));
        }

        var file = await filesRepository.GetByIdAsync(id.Value, cancellationToken);
        if (file is null || !fileAccessPolicy.CanUpdate(file))
        {
            return Result.Failure(Error.NotFound("File was not found."));
        }

        var upload = file.BeginContentUpload(FileContentTimestamp.Create(timeProvider.GetUtcNow()));
        if (upload.IsFailure)
        {
            return Result.Failure(upload.Errors);
        }

        filesRepository.Update(file);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await fileStorage.UploadAsync(
            key: upload.Value.StorageKey.Value,
            content: command.Content,
            contentType: contentType.Value.Value,
            cancellationToken: cancellationToken);

        var metadata = await fileStorage.GetMetadataAsync(upload.Value.StorageKey.Value, cancellationToken);
        if (metadata is null || metadata.ContentType != contentType.Value.Value || metadata.Size != size.Value.Value)
        {
            return Result.Failure(Error.Conflict("Uploaded file content does not match the requested metadata."));
        }

        var result = file.ReplaceContent(
            contentId: upload.Value.Id,
            contentType: contentType.Value,
            size: size.Value,
            now: FileContentTimestamp.Create(timeProvider.GetUtcNow()));
        if (result.IsFailure)
        {
            return result;
        }

        filesRepository.Update(file);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
