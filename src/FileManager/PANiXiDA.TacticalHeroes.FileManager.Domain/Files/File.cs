using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

namespace PANiXiDA.TacticalHeroes.FileManager.Domain.Files;

public sealed class File : AggregateRoot<FileId>
{
    private File(FileId id, FileName name, FilePurpose purpose)
        : base(id)
    {
        Name = name;
        Purpose = purpose;
        Status = FileStatus.PendingUpload;
    }

    public FileName Name { get; private set; }
    public FilePurpose Purpose { get; }
    public FileStatus Status { get; private set; }
    public FileContent? Content { get; private set; }

    public static File Create(FileName name, FilePurpose purpose)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(purpose);

        return new File(id: FileId.New(), name: name, purpose: purpose);
    }

    public Result Rename(FileName name)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (Status == FileStatus.Deleting || Status == FileStatus.Deleted)
        {
            return Result.Failure(
                Error.Conflict("A file being deleted cannot be renamed."));
        }

        Name = name;

        return Result.Success();
    }

    public Result CompleteUpload(FileContent content)
    {
        ArgumentNullException.ThrowIfNull(content);

        if (Status == FileStatus.Ready)
        {
            return Content == content
                ? Result.Success()
                : Result.Failure(
                    Error.Conflict("File content cannot be replaced after upload."));
        }

        if (Status != FileStatus.PendingUpload)
        {
            return Result.Failure(
                Error.Conflict("Only pending uploads can be completed."));
        }

        Content = content;
        Status = FileStatus.Ready;

        return Result.Success();
    }

    public Result BeginDeletion()
    {
        if (Status == FileStatus.Deleting || Status == FileStatus.Deleted)
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
