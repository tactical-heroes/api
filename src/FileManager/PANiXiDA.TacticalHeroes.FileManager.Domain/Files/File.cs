using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;

namespace PANiXiDA.TacticalHeroes.FileManager.Domain.Files;

public sealed class File : AggregateRoot<FileId>
{
    private File(
        FileId id,
        FileName name,
        FileType type)
        : base(id)
    {
        Name = name;
        Type = type;
        Status = FileStatus.PendingUpload;
    }

    public FileName Name { get; private set; }
    public FileType Type { get; }
    public FolderId? FolderId { get; private set; }
    public FileStatus Status { get; private set; }
    public FileContentType? ContentType { get; private set; }
    public FileSize? Size { get; private set; }

    public static File Create(
        FileName name,
        FileType type)
    {
        return new File(
            id: FileId.New(),
            name: name,
            type: type);
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
