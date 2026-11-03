using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.Core.Application.Storage;

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

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.CreateUploadUrl;

public sealed class CreateFileUploadUrlHandler(
    IFilesRepository filesRepository,
    IFoldersRepository foldersRepository,
    IFolderHierarchyLock folderHierarchyLock,
    IFileStorage fileStorage,
    FileAccessPolicy fileAccessPolicy,
    FolderAccessPolicy folderAccessPolicy)
    : ICommandHandler<CreateFileUploadUrlCommand, Result<FileUploadUrl>>, IRequireAuthorization, IRequireTransaction
{
    public async Task<Result<FileUploadUrl>> HandleAsync(
        CreateFileUploadUrlCommand command,
        CancellationToken cancellationToken)
    {
        var name = FileName.Create(command.Name);
        var type = FileType.Create(command.Type);
        var contentType = FileContentType.Create(command.ContentType);
        var size = FileSize.Create(command.Size);
        var ownerId = UserId.Create(command.UserId);
        var folderId = FolderId.Create(command.FolderId);
        var validation = Result.Combine(name, type, contentType, size, ownerId, folderId);
        if (validation.IsFailure)
        {
            return Result.Failure<FileUploadUrl>(validation.Errors);
        }

        var created = File.Create(name: name.Value, type: type.Value, userId: ownerId.Value);
        if (created.IsFailure)
        {
            return Result.Failure<FileUploadUrl>(created.Errors);
        }

        var file = created.Value;
        if (!fileAccessPolicy.CanCreate(file))
        {
            return Result.Failure<FileUploadUrl>(Error.Forbidden("Access to create the file is denied."));
        }

        if (folderId.Value is { } targetId)
        {
            await folderHierarchyLock.AcquireAsync(cancellationToken);
            var folder = await foldersRepository.GetByIdAsync(targetId, cancellationToken);
            if (folder is null || !folderAccessPolicy.CanCreate(folder))
            {
                return Result.Failure<FileUploadUrl>(Error.NotFound("Folder was not found."));
            }

            var moved = file.MoveTo(folder);
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
}
