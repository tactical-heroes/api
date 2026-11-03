using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.Core.Application.Storage;

using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.CreatePersonal;

public sealed class CreatePersonalFileHandler(
    IFilesRepository filesRepository,
    IFoldersRepository foldersRepository,
    IFolderHierarchyLock folderHierarchyLock,
    IFileStorage fileStorage,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : ICommandHandler<CreatePersonalFileCommand, Result<Guid>>, IRequireAuthorization
{
    public async Task<Result<Guid>> HandleAsync(
        CreatePersonalFileCommand command,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId || userId == Guid.Empty)
        {
            return Result.Failure<Guid>(Error.Unauthorized("A user account is required."));
        }

        var nameResult = FileName.Create(command.Name);
        var contentTypeResult = FileContentType.Create(command.ContentType);
        var sizeResult = FileSize.Create(command.Size);
        var userIdResult = UserId.Create(userId);
        var folderIdResult = FolderId.Create(command.FolderId);
        var validationResult = Result.Combine(
            nameResult,
            contentTypeResult,
            sizeResult,
            userIdResult,
            folderIdResult);

        if (validationResult.IsFailure)
        {
            return Result.Failure<Guid>(validationResult.Errors);
        }

        if (command.Content is not { CanRead: true })
        {
            return Result.Failure<Guid>(Error.Validation("File content must be readable."));
        }

        var file = File.Create(
            name: nameResult.Value,
            type: FileType.Personal,
            userId: userIdResult.Value).Value;

        var reservation = Result.Success();
        await unitOfWork.ExecuteInTransactionAsync(
            action: async token =>
            {
                if (folderIdResult.Value is { } folderId)
                {
                    await folderHierarchyLock.AcquireAsync(token);
                    var folder = await foldersRepository.GetByIdAsync(folderId, token);
                    if (folder is null || folder.UserId != file.UserId)
                    {
                        reservation = Result.Failure(Error.NotFound("Folder was not found."));
                        return;
                    }

                    reservation = file.MoveTo(folder);
                    if (reservation.IsFailure)
                    {
                        return;
                    }
                }

                filesRepository.Add(aggregateRoot: file);
                await unitOfWork.SaveChangesAsync(token);
            },
            cancellationToken: cancellationToken);
        if (reservation.IsFailure)
        {
            return Result.Failure<Guid>(reservation.Errors);
        }

        await fileStorage.UploadAsync(
            key: file.StorageKey.Value,
            content: command.Content,
            contentType: contentTypeResult.Value.Value,
            cancellationToken: cancellationToken);

        var completeResult = file.CompleteUpload(
            contentType: contentTypeResult.Value,
            size: sizeResult.Value);
        if (completeResult.IsFailure)
        {
            return Result.Failure<Guid>(completeResult.Errors);
        }

        filesRepository.Update(aggregateRoot: file);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(file.Id.Value);
    }
}
