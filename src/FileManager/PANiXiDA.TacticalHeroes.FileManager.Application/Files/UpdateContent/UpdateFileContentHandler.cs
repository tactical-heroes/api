using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.Core.Application.Storage;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.UpdateContent;

public sealed class UpdateFileContentHandler(
    IFilesRepository filesRepository,
    IFileStorage fileStorage,
    IUnitOfWork unitOfWork,
    FileAccessPolicy fileAccessPolicy,
    IFileContentCleanupScheduler cleanupScheduler)
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

        var pendingKey = started.Value;
        filesRepository.Update(file);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await fileStorage.UploadAsync(
            key: pendingKey.Value,
            content: command.Content,
            contentType: contentType.Value.Value,
            cancellationToken: cancellationToken);

        var metadata = await fileStorage.GetMetadataAsync(pendingKey.Value, cancellationToken);
        if (metadata is null || metadata.ContentType != contentType.Value.Value || metadata.Size != size.Value.Value)
        {
            return Result.Failure(Error.Conflict("Uploaded file content does not match the requested metadata."));
        }

        var result = file.CompleteContentUpdate(
            pendingStorageKey: pendingKey,
            contentType: contentType.Value,
            size: size.Value);
        if (result.IsFailure)
        {
            return Result.Failure(result.Errors);
        }

        await unitOfWork.ExecuteInTransactionAsync(
            action: async token =>
            {
                filesRepository.Update(file);
                await cleanupScheduler.ScheduleAsync(result.Value, token);
                await unitOfWork.SaveChangesAsync(token);
            },
            cancellationToken: cancellationToken);

        return Result.Success();
    }
}
