using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

namespace PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;

public sealed class Folder : AggregateRoot<FolderId>
{
    private Folder(
        FolderId id,
        FolderName name,
        FileType allowedFileType,
        FolderId? parentId,
        UserId? userId)
        : base(id)
    {
        Name = name;
        AllowedFileType = allowedFileType;
        ParentId = parentId;
        UserId = userId;
    }

    public FolderName Name { get; private set; }
    public FileType AllowedFileType { get; private set; }
    public UserId? UserId { get; private set; }
    public FolderId? ParentId { get; private set; }

    public static Result<Folder> Create(
        FolderName name,
        FileType allowedFileType,
        UserId? userId)
    {
        if (allowedFileType == FileType.Personal && userId is null)
        {
            return Result.Failure<Folder>(
                Error.Validation("Personal folders require a user id.")
                    .WithField(nameof(UserId)));
        }

        if (allowedFileType != FileType.Personal && userId is not null)
        {
            return Result.Failure<Folder>(
                Error.Validation("Only personal folders can have a user id.")
                    .WithField(nameof(UserId)));
        }

        return Result.Success(new Folder(
            id: FolderId.New(),
            name: name,
            allowedFileType: allowedFileType,
            parentId: null,
            userId: userId));
    }

    public Folder CreateChild(FolderName name)
    {
        return new Folder(
            id: FolderId.New(),
            name: name,
            allowedFileType: AllowedFileType,
            parentId: Id,
            userId: UserId);
    }

    public void Rename(FolderName name)
    {
        Name = name;
    }
}
