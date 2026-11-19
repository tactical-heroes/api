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
    public FileVersionId? VersionId { get; private set; }
    public FileContentUpdateAttemptId? ContentUpdateAttemptId { get; private set; }

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
        if ((Status != FileStatus.Ready && Status != FileStatus.Updating) || ContentType is not { } contentType)
        {
            return Result.Failure<FileContentType>(Error.Conflict("File is not ready for download."));
        }

        return Result.Success(contentType);
    }

    public Result Rename(FileName name)
    {
        if (Status == FileStatus.Updating)
        {
            return Result.Failure(Error.Conflict("A file being updated cannot be renamed."));
        }

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
        if (Status == FileStatus.Updating)
        {
            return Result.Failure(Error.Conflict("A file being updated cannot be moved."));
        }

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
        FileSize size,
        FileVersionId? versionId = null)
    {
        if (Status == FileStatus.Ready)
        {
            if (ContentType != contentType
                || Size != size
                || (versionId is not null && VersionId is not null && VersionId != versionId))
            {
                return Result.Failure(Error.Conflict("File content cannot be replaced after upload."));
            }

            VersionId ??= versionId;

            return Result.Success();
        }

        if (Status != FileStatus.PendingUpload)
        {
            return Result.Failure(
                Error.Conflict("Only pending uploads can be completed."));
        }

        ContentType = contentType;
        Size = size;
        VersionId = versionId;
        Status = FileStatus.Ready;

        return Result.Success();
    }

    public Result<FileContentUpdateAttemptId> BeginContentUpdate(FileVersionId versionId)
    {
        if (Status != FileStatus.Ready)
        {
            return Result.Failure<FileContentUpdateAttemptId>(
                Error.Conflict("Only ready file content can be replaced."));
        }

        if (VersionId is not null && VersionId != versionId)
        {
            return Result.Failure<FileContentUpdateAttemptId>(
                Error.Conflict("The active storage version has changed."));
        }

        VersionId = versionId;
        ContentUpdateAttemptId = FileContentUpdateAttemptId.New();
        Status = FileStatus.Updating;

        return Result.Success(ContentUpdateAttemptId);
    }

    public Result CompleteContentUpdate(
        FileContentUpdateAttemptId contentUpdateAttemptId,
        FileContentType contentType,
        FileSize size,
        FileVersionId versionId)
    {
        if (Status != FileStatus.Updating || ContentUpdateAttemptId != contentUpdateAttemptId || VersionId == versionId)
        {
            return Result.Failure(Error.Conflict("File content upload is no longer available."));
        }

        ContentType = contentType;
        Size = size;
        VersionId = versionId;
        ContentUpdateAttemptId = null;
        Status = FileStatus.Ready;

        return Result.Success();
    }

    public Result CancelContentUpdate(FileContentUpdateAttemptId contentUpdateAttemptId)
    {
        if (Status != FileStatus.Updating || ContentUpdateAttemptId != contentUpdateAttemptId)
        {
            return Result.Failure(Error.Conflict("File content upload is no longer available."));
        }

        ContentUpdateAttemptId = null;
        Status = FileStatus.Ready;

        return Result.Success();
    }

    public Result BeginDeletion()
    {
        if (Status == FileStatus.Updating)
        {
            return Result.Failure(Error.Conflict("A file being updated cannot be deleted."));
        }

        if (Status == FileStatus.Deleting)
        {
            return Result.Success();
        }

        Status = FileStatus.Deleting;

        return Result.Success();
    }
}
