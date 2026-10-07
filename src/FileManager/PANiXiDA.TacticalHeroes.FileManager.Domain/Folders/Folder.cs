using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;

namespace PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;

public sealed class Folder : AggregateRoot<FolderId>
{
    private Folder(
        FolderId id,
        FolderName name,
        FileType allowedFileType,
        FolderId? parentId)
        : base(id)
    {
        Name = name;
        AllowedFileType = allowedFileType;
        ParentId = parentId;
    }

    public FolderName Name { get; private set; }
    public FileType AllowedFileType { get; }
    public FolderId? ParentId { get; }

    public static Folder Create(
        FolderName name,
        FileType allowedFileType)
    {
        return new Folder(
            id: FolderId.New(),
            name: name,
            allowedFileType: allowedFileType,
            parentId: null);
    }

    public Folder CreateChild(FolderName name)
    {
        return new Folder(
            id: FolderId.New(),
            name: name,
            allowedFileType: AllowedFileType,
            parentId: Id);
    }

    public void Rename(FolderName name)
    {
        Name = name;
    }
}
