using PANiXiDA.Core.Application.Storage;
using PANiXiDA.Core.Application.Storage.Models;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Services;

public sealed class FileCreationService(
    IFilesRepository filesRepository,
    IFoldersRepository foldersRepository,
    IFolderHierarchyLock folderHierarchyLock,
    IFileStorage fileStorage,
    IUnitOfWork unitOfWork,
    FileAccessPolicy fileAccessPolicy,
    FolderAccessPolicy folderAccessPolicy)
{
    public async Task<Result<Guid>> CreateAsync(
        string name,
        FileType type,
        Guid? userId,
        FileMetadata metadata,
        Stream content,
        Guid? folderId,
        CancellationToken cancellationToken)
    {
        var nameResult = FileName.Create(name);
        var contentTypeResult = FileContentType.Create(metadata.ContentType);
        var sizeResult = FileSize.Create(metadata.Size);
        var userIdResult = UserId.Create(userId);
        var folderIdResult = FolderId.Create(folderId);
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

        if (content is not { CanRead: true })
        {
            return Result.Failure<Guid>(Error.Validation("File content must be readable."));
        }

        var created = File.Create(name: nameResult.Value, type: type, userId: userIdResult.Value);
        if (created.IsFailure)
        {
            return Result.Failure<Guid>(created.Errors);
        }

        var file = created.Value;
        if (!fileAccessPolicy.CanCreate(file))
        {
            return Result.Failure<Guid>(Error.Forbidden("Access to create the file is denied."));
        }

        var reservation = Result.Success();
        await unitOfWork.ExecuteInTransactionAsync(
            action: async token =>
            {
                if (folderIdResult.Value is { } targetId)
                {
                    await folderHierarchyLock.AcquireAsync(token);
                    var folder = await foldersRepository.GetByIdAsync(targetId, token);
                    if (folder is null
                        || !folderAccessPolicy.CanCreate(folder)
                        || folder.UserId != file.UserId
                        || folder.AllowedFileType != file.Type)
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
            content: content,
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
