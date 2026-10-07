using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;

namespace PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;

public sealed class Folder : AggregateRoot<FolderId>
{
    private Folder(
        FolderId id,
        FolderName name,
        FileType type,
        FolderId? parentId)
        : base(id)
    {
        Name = name;
        Type = type;
        ParentId = parentId;
    }

    public FolderName Name { get; private set; }
    public FileType Type { get; }
    public FolderId? ParentId { get; }

    public static Folder Create(
        FolderName name,
        FileType type)
    {
        return new Folder(
            id: FolderId.New(),
            name: name,
            type: type,
            parentId: null);
    }

    public Folder CreateChild(FolderName name)
    {
        return new Folder(
            id: FolderId.New(),
            name: name,
            type: Type,
            parentId: Id);
    }

    public void Rename(FolderName name)
    {
        Name = name;
    }
}
