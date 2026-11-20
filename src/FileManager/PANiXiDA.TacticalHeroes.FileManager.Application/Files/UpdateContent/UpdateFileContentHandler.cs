using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.Core.Application.Storage;
using PANiXiDA.Core.Application.Storage.Constants;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
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

        var started = file.BeginContentUpdate();
        if (started.IsFailure)
        {
            return Result.Failure(started.Errors);
        }

        filesRepository.Update(file);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var uploaded = await fileStorage.UploadAsync(
            key: file.StorageKey.Value,
            content: command.Content,
            contentType: contentType.Value.Value,
            metadata: new Dictionary<string, string>
            {
                [FileMetadataNames.ContentUpdateAttemptId] = started.Value.Value.ToString()
            },
            cancellationToken: cancellationToken);
        var uploadedVersion = FileVersionId.Create(uploaded.VersionId);
        if (uploadedVersion.IsFailure)
        {
            return Result.Failure(Error.Conflict("Uploaded file content has no storage version."));
        }

        var metadata = await fileStorage.GetMetadataAsync(
            key: file.StorageKey.Value,
            versionId: uploadedVersion.Value.Value,
            cancellationToken: cancellationToken);
        if (!FileContentUpdatePolicy.CanAccept(
            metadata: metadata,
            activeVersion: file.VersionId,
            contentUpdateAttemptId: started.Value)
            || metadata.ContentType != contentType.Value.Value
            || metadata.Size != size.Value.Value
            || metadata.VersionId != uploadedVersion.Value.Value)
        {
            return Result.Failure(Error.Conflict("Uploaded file content does not match the requested metadata."));
        }

        var completed = file.CompleteContentUpdate(
            contentUpdateAttemptId: started.Value,
            contentType: contentType.Value,
            size: size.Value,
            versionId: uploadedVersion.Value);
        if (completed.IsFailure)
        {
            return completed;
        }

        filesRepository.Update(file);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
