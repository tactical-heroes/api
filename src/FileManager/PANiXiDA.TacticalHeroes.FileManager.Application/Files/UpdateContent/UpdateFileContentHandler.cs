using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.Core.Application.Storage;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.UpdateContent;

public sealed class UpdateFileContentHandler(
    IFilesRepository filesRepository,
    IFileStorage fileStorage,
    IUnitOfWork unitOfWork,
    FileAccessPolicy fileAccessPolicy)
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

        if (file.Status != FileStatus.Ready)
        {
            return Result.Failure(Error.Conflict("Only ready file content can be replaced."));
        }

        var current = await fileStorage.GetMetadataAsync(file.StorageKey.Value, cancellationToken);
        var previousVersion = FileVersionId.Create(current?.VersionId);
        if (previousVersion.IsFailure)
        {
            return Result.Failure(Error.Conflict("File content must exist in storage with bucket versioning enabled."));
        }

        var started = file.BeginContentUpdate(previousVersion.Value);
        if (started.IsFailure)
        {
            return started;
        }

        filesRepository.Update(file);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await fileStorage.UploadAsync(
            key: file.StorageKey.Value,
            content: command.Content,
            contentType: contentType.Value.Value,
            cancellationToken: cancellationToken);

        var metadata = await fileStorage.GetMetadataAsync(file.StorageKey.Value, cancellationToken);
        if (metadata is null || metadata.ContentType != contentType.Value.Value || metadata.Size != size.Value.Value)
        {
            return Result.Failure(Error.Conflict("Uploaded file content does not match the requested metadata."));
        }

        var version = FileVersionId.Create(metadata.VersionId);
        if (version.IsFailure)
        {
            return Result.Failure(Error.Conflict("Uploaded file content has no storage version."));
        }

        var result = file.CompleteContentUpdate(
            contentType: contentType.Value,
            size: size.Value,
            versionId: version.Value);
        if (result.IsFailure)
        {
            return result;
        }

        filesRepository.Update(file);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
