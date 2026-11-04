using PANiXiDA.Core.Application.Storage;
using PANiXiDA.Core.Application.Storage.Models;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Common.Models;
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
        var contentType = FileContentType.Create(metadata.ContentType);
        var size = FileSize.Create(metadata.Size);
        var targetId = FolderId.Create(folderId);
        var validation = Result.Combine(contentType, size, targetId);
        if (validation.IsFailure)
        {
            return Result.Failure<Guid>(validation.Errors);
        }

        if (content is not { CanRead: true })
        {
            return Result.Failure<Guid>(Error.Validation("File content must be readable."));
        }

        var created = CreateFile(name: name, type: type, userId: userId);
        if (created.IsFailure)
        {
            return Result.Failure<Guid>(created.Errors);
        }

        var file = created.Value;
        var reservation = Result.Success();
        await unitOfWork.ExecuteInTransactionAsync(
            action: async token =>
            {
                if (targetId.Value is { } destinationId)
                {
                    var folder = await GetFolderAsync(destinationId, token);
                    if (folder.IsFailure)
                    {
                        reservation = Result.Failure(folder.Errors);
                        return;
                    }

                    if (folder.Value.UserId != file.UserId || folder.Value.AllowedFileType != file.Type)
                    {
                        reservation = Result.Failure(Error.NotFound("Folder was not found."));
                        return;
                    }

                    reservation = file.MoveTo(folder.Value);
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
            contentType: contentType.Value.Value,
            cancellationToken: cancellationToken);

        var completed = file.CompleteUpload(contentType: contentType.Value, size: size.Value);
        if (completed.IsFailure)
        {
            return Result.Failure<Guid>(completed.Errors);
        }

        filesRepository.Update(aggregateRoot: file);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(file.Id.Value);
    }

    public async Task<Result<FileUploadUrl>> CreateUploadUrlAsync(
        string name,
        FileType type,
        Guid? userId,
        FileMetadata metadata,
        Guid? folderId,
        CancellationToken cancellationToken)
    {
        var contentType = FileContentType.Create(metadata.ContentType);
        var size = FileSize.Create(metadata.Size);
        var targetId = FolderId.Create(folderId);
        var validation = Result.Combine(contentType, size, targetId);
        if (validation.IsFailure)
        {
            return Result.Failure<FileUploadUrl>(validation.Errors);
        }

        var created = CreateFile(name: name, type: type, userId: userId);
        if (created.IsFailure)
        {
            return Result.Failure<FileUploadUrl>(created.Errors);
        }

        var file = created.Value;
        if (targetId.Value is { } destinationId)
        {
            var folder = await GetFolderAsync(destinationId, cancellationToken);
            if (folder.IsFailure)
            {
                return Result.Failure<FileUploadUrl>(folder.Errors);
            }

            var moved = file.MoveTo(folder.Value);
            if (moved.IsFailure)
            {
                return Result.Failure<FileUploadUrl>(moved.Errors);
            }
        }

        var upload = await fileStorage.GetPresignedUploadUrlAsync(
            key: file.StorageKey.Value,
            contentType: contentType.Value.Value,
            size: size.Value.Value,
            cancellationToken: cancellationToken);
        filesRepository.Add(aggregateRoot: file);

        return Result.Success(new FileUploadUrl(file.Id.Value, upload));
    }

    private Result<File> CreateFile(string name, FileType type, Guid? userId)
    {
        var fileName = FileName.Create(name);
        var ownerId = UserId.Create(userId);
        var validation = Result.Combine(fileName, ownerId);
        if (validation.IsFailure)
        {
            return Result.Failure<File>(validation.Errors);
        }

        var created = File.Create(name: fileName.Value, type: type, userId: ownerId.Value);
        if (created.IsFailure)
        {
            return created;
        }

        return fileAccessPolicy.CanCreate(created.Value)
            ? created
            : Result.Failure<File>(Error.Forbidden("Access to create the file is denied."));
    }

    private async Task<Result<Folder>> GetFolderAsync(FolderId folderId, CancellationToken cancellationToken)
    {
        await folderHierarchyLock.AcquireAsync(cancellationToken);
        var folder = await foldersRepository.GetByIdAsync(folderId, cancellationToken);

        return folder is not null && folderAccessPolicy.CanCreate(folder)
            ? Result.Success(folder)
            : Result.Failure<Folder>(Error.NotFound("Folder was not found."));
    }
}
