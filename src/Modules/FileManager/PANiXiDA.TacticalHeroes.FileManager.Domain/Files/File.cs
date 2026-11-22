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
    public FileOperationAttemptId? OperationAttemptId { get; private set; }

    public static Result<File> Create(
        FileName name,
        FileType type,
        UserId? userId)
    {
        var validation = ValidateOwnership(type, userId);
        if (validation.IsFailure)
        {
            return Result.Failure<File>(validation.Errors);
        }

        var id = FileId.New();
        var storageKey = FileStorageKey.Create(id: id, type: type, userId: userId).Value;

        return Result.Success(new File(
            id: id,
            name: name,
            type: type,
            userId: userId,
            storageKey: storageKey));
    }

    public Result Update(FileName name, FileType type, UserId? userId, Folder? folder)
    {
        if (Status == FileStatus.Relocating)
        {
            return Result.Failure(Error.Conflict("A file being relocated cannot be updated."));
        }

        if (Status == FileStatus.Updating)
        {
            return Result.Failure(Error.Conflict("File metadata cannot be changed while content is being updated."));
        }

        if (Status == FileStatus.Deleting)
        {
            return Result.Failure(Error.Conflict("A file being deleted cannot be updated."));
        }

        var validation = Result.Combine(
            ValidateOwnership(type, userId),
            ValidateFolder(folder: folder, type: type, userId: userId));
        if (validation.IsFailure)
        {
            return validation;
        }

        if (Type != type || UserId != userId)
        {
            if (Status != FileStatus.Ready || VersionId is null || ContentType is null || Size is null)
            {
                return Result.Failure(Error.Conflict("Only ready files can change type or owner."));
            }

            OperationAttemptId = FileOperationAttemptId.New();
            Status = FileStatus.Relocating;
        }

        Name = name;
        Type = type;
        UserId = userId;
        FolderId = folder?.Id;

        return Result.Success();
    }

    public Result CompleteRelocation(FileOperationAttemptId attemptId, FileVersionId versionId)
    {
        if (Status != FileStatus.Relocating || OperationAttemptId != attemptId)
        {
            return Result.Failure(Error.Conflict("File relocation is no longer available."));
        }

        StorageKey = FileStorageKey.Create(id: Id, type: Type, userId: UserId).Value;
        VersionId = versionId;
        OperationAttemptId = null;
        Status = FileStatus.Ready;

        return Result.Success();
    }

    public Result<FileContentType> GetDownloadContentType()
    {
        if ((Status != FileStatus.Ready && Status != FileStatus.Updating && Status != FileStatus.Relocating)
            || ContentType is not { } contentType)
        {
            return Result.Failure<FileContentType>(Error.Conflict("File is not ready for download."));
        }

        return Result.Success(contentType);
    }

    public Result Rename(FileName name)
    {
        if (Status == FileStatus.Relocating)
        {
            return Result.Failure(Error.Conflict("A file being relocated cannot be renamed."));
        }

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
        if (Status == FileStatus.Relocating)
        {
            return Result.Failure(Error.Conflict("A file being relocated cannot be moved."));
        }

        if (Status == FileStatus.Updating)
        {
            return Result.Failure(Error.Conflict("A file being updated cannot be moved."));
        }

        if (Status == FileStatus.Deleting)
        {
            return Result.Failure(
                Error.Conflict("A file being deleted cannot be moved."));
        }

        var validation = ValidateFolder(folder: folder, type: Type, userId: UserId);
        if (validation.IsFailure)
        {
            return validation;
        }

        FolderId = folder?.Id;

        return Result.Success();
    }

    public Result CompleteUpload(
        FileContentType contentType,
        FileSize size,
        FileVersionId versionId)
    {
        if (Status == FileStatus.Ready)
        {
            if (ContentType != contentType
                || Size != size
                || VersionId != versionId)
            {
                return Result.Failure(Error.Conflict("File content cannot be replaced after upload."));
            }

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

    public Result<FileOperationAttemptId> BeginContentUpdate()
    {
        if (Status != FileStatus.Ready)
        {
            return Result.Failure<FileOperationAttemptId>(
                Error.Conflict("Only ready file content can be replaced."));
        }

        OperationAttemptId = FileOperationAttemptId.New();
        Status = FileStatus.Updating;

        return Result.Success(OperationAttemptId);
    }

    public Result CompleteContentUpdate(
        FileOperationAttemptId contentUpdateAttemptId,
        FileContentType contentType,
        FileSize size,
        FileVersionId versionId)
    {
        if (Status != FileStatus.Updating || OperationAttemptId != contentUpdateAttemptId || VersionId == versionId)
        {
            return Result.Failure(Error.Conflict("File content upload is no longer available."));
        }

        ContentType = contentType;
        Size = size;
        VersionId = versionId;
        OperationAttemptId = null;
        Status = FileStatus.Ready;

        return Result.Success();
    }

    public Result CancelContentUpdate(FileOperationAttemptId contentUpdateAttemptId)
    {
        if (Status != FileStatus.Updating || OperationAttemptId != contentUpdateAttemptId)
        {
            return Result.Failure(Error.Conflict("File content upload is no longer available."));
        }

        OperationAttemptId = null;
        Status = FileStatus.Ready;

        return Result.Success();
    }

    public Result BeginDeletion()
    {
        if (Status == FileStatus.Relocating)
        {
            return Result.Failure(Error.Conflict("A file being relocated cannot be deleted."));
        }

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

    private static Result ValidateOwnership(FileType type, UserId? userId)
    {
        if (type == FileType.Personal && userId is null)
        {
            return Result.Failure(
                Error.Validation("Personal files require a user id.")
                    .WithField(nameof(UserId)));
        }

        if (type != FileType.Personal && userId is not null)
        {
            return Result.Failure(
                Error.Validation("Only personal files can have a user id.")
                    .WithField(nameof(UserId)));
        }

        return Result.Success();
    }

    private static Result ValidateFolder(Folder? folder, FileType type, UserId? userId)
    {
        if (folder is not null)
        {
            if (type != folder.AllowedFileType)
            {
                return Result.Failure(
                    Error.Validation("File type must match the folder's allowed file type.")
                        .WithField(nameof(FolderId)));
            }

            if (userId != folder.UserId)
            {
                return Result.Failure(
                    Error.Validation("File and folder user ids must match.")
                        .WithField(nameof(FolderId)));
            }
        }

        return Result.Success();
    }
}
