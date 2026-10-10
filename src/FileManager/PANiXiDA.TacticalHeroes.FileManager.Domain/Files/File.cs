using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

namespace PANiXiDA.TacticalHeroes.FileManager.Domain.Files;

public sealed class File : AggregateRoot<FileId>
{
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

    public Result Rename(FileName name)
    {
        if (Status.IsDeletingOrDeleted)
        {
            return Result.Failure(
                Error.Conflict("A file being deleted cannot be renamed."));
        }

        Name = name;

        return Result.Success();
    }

    public Result MoveTo(Folder folder)
    {
        if (Status.IsDeletingOrDeleted)
        {
            return Result.Failure(
                Error.Conflict("A file being deleted cannot be moved."));
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

    public Result BeginDeletion()
    {
        if (Status.IsDeletingOrDeleted)
        {
            return Result.Success();
        }

        Status = FileStatus.Deleting;

        return Result.Success();
    }

    public Result CompleteDeletion()
    {
        if (Status == FileStatus.Deleted)
        {
            return Result.Success();
        }

        if (Status != FileStatus.Deleting)
        {
            return Result.Failure(
                Error.Conflict("File deletion must begin before it can be completed."));
        }

        Status = FileStatus.Deleted;

        return Result.Success();
    }
}
