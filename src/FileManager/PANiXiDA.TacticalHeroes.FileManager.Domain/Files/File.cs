using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Entities.FileContents;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Entities.FileContents.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

namespace PANiXiDA.TacticalHeroes.FileManager.Domain.Files;

public sealed class File : AggregateRoot<FileId>
{
    private readonly List<FileContent> _unusedContents = [];

    private File(
        FileId id,
        FileName name,
        FileType type,
        UserId? userId,
        FileStorageKey storageKey)
        : base(id)
    {
        Name = name;
        Type = type;
        UserId = userId;
        StorageKey = storageKey;
        Status = FileStatus.PendingUpload;
    }

    public FileName Name { get; private set; }
    public FileStorageKey StorageKey { get; private set; }
    public FileType Type { get; private set; }
    public UserId? UserId { get; private set; }
    public FolderId? FolderId { get; private set; }
    public FileStatus Status { get; private set; }
    public FileContentType? ContentType { get; private set; }
    public FileSize? Size { get; private set; }
    public IReadOnlyCollection<FileContent> UnusedContents => _unusedContents.AsReadOnly();

    public static Result<File> Create(
        FileName name,
        FileType type,
        UserId? userId)
    {
        if (type == FileType.Personal && userId is null)
        {
            return Result.Failure<File>(
                Error.Validation("Personal files require a user id.")
                    .WithField(nameof(UserId)));
        }

        if (type != FileType.Personal && userId is not null)
        {
            return Result.Failure<File>(
                Error.Validation("Only personal files can have a user id.")
                    .WithField(nameof(UserId)));
        }

        var id = FileId.New();
        var prefix = type.Name.ToLowerInvariant();
        if (userId is { } ownerId)
        {
            prefix = $"{prefix}/{ownerId.Value:D}";
        }

        var storageKey = FileStorageKey.Create($"{prefix}/{id.Value:D}").Value;

        return Result.Success(new File(
            id: id,
            name: name,
            type: type,
            userId: userId,
            storageKey: storageKey));
    }

    public Result<FileContentType> GetDownloadContentType()
    {
        if (Status != FileStatus.Ready || ContentType is not { } contentType)
        {
            return Result.Failure<FileContentType>(Error.Conflict("File is not ready for download."));
        }

        return Result.Success(contentType);
    }

    public Result Rename(FileName name)
    {
        if (Status == FileStatus.Deleting)
        {
            return Result.Failure(
                Error.Conflict("A file being deleted cannot be renamed."));
        }

        Name = name;

        return Result.Success();
    }

    public Result MoveTo(Folder? folder)
    {
        if (Status == FileStatus.Deleting)
        {
            return Result.Failure(
                Error.Conflict("A file being deleted cannot be moved."));
        }

        if (folder is null)
        {
            FolderId = null;

            return Result.Success();
        }

        if (Type != folder.AllowedFileType)
        {
            return Result.Failure(
                Error.Validation("File type must match the folder's allowed file type.")
                    .WithField(nameof(FolderId)));
        }

        if (UserId != folder.UserId)
        {
            return Result.Failure(
                Error.Validation("File and folder user ids must match.")
                    .WithField(nameof(FolderId)));
        }

        FolderId = folder.Id;

        return Result.Success();
    }

    public Result CompleteUpload(
        FileContentType contentType,
        FileSize size)
    {
        if (Status == FileStatus.Ready)
        {
            return ContentType == contentType && Size == size
                ? Result.Success()
                : Result.Failure(
                    Error.Conflict("File content cannot be replaced after upload."));
        }

        if (Status != FileStatus.PendingUpload)
        {
            return Result.Failure(
                Error.Conflict("Only pending uploads can be completed."));
        }

        ContentType = contentType;
        Size = size;
        Status = FileStatus.Ready;

        return Result.Success();
    }

    public Result<FileContent> BeginContentUpload(FileContentTimestamp now)
    {
        if (Status != FileStatus.Ready)
        {
            return Result.Failure<FileContent>(Error.Conflict("Only ready file content can be replaced."));
        }

        if (_unusedContents.Any(content => content.DeletionStartedAt is null))
        {
            return Result.Failure<FileContent>(Error.Conflict("A file content upload is already in progress."));
        }

        var key = FileStorageKey.Create($"contents/{Id.Value:D}/{Guid.CreateVersion7():D}").Value;
        var content = FileContent.Create(fileId: Id, storageKey: key, registeredAt: now);
        _unusedContents.Add(content);

        return Result.Success(content);
    }

    public Result ReplaceContent(
        FileContentId contentId,
        FileContentType contentType,
        FileSize size,
        FileContentTimestamp now)
    {
        var content = _unusedContents.SingleOrDefault(item => item.Id == contentId);
        if (Status != FileStatus.Ready || content is null || content.DeletionStartedAt is not null)
        {
            return Result.Failure(Error.Conflict("File content upload is no longer available."));
        }

        var previous = FileContent.Create(fileId: Id, storageKey: StorageKey, registeredAt: now);
        previous.BeginDeletion(now);
        _unusedContents.Remove(content);
        _unusedContents.Add(previous);
        StorageKey = content.StorageKey;
        ContentType = contentType;
        Size = size;

        return Result.Success();
    }

    public Result BeginContentDeletion(FileContentId contentId, FileContentTimestamp now)
    {
        var content = _unusedContents.SingleOrDefault(item => item.Id == contentId);
        if (content is null || content.StorageKey == StorageKey)
        {
            return Result.Failure(Error.Conflict("Active or unknown file content cannot be deleted."));
        }

        content.BeginDeletion(now);

        return Result.Success();
    }

    public Result ForgetContent(FileContentId contentId)
    {
        var content = _unusedContents.SingleOrDefault(item => item.Id == contentId);
        if (content is null || content.DeletionStartedAt is null || content.StorageKey == StorageKey)
        {
            return Result.Failure(Error.Conflict("Only deleted unused file content can be forgotten."));
        }

        _unusedContents.Remove(content);

        return Result.Success();
    }

    public Result BeginDeletion()
    {
        if (Status == FileStatus.Deleting)
        {
            return Result.Success();
        }

        Status = FileStatus.Deleting;

        return Result.Success();
    }
}
