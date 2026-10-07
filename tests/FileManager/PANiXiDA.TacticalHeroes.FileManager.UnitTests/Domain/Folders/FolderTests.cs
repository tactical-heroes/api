using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Domain.Folders;

public sealed class FolderTests
{
    [Fact(DisplayName = "Folder should have no parent when allowed file type is known")]
    public void Create_Should_ReturnRootFolder_When_AllowedFileTypeIsKnown()
    {
        var name = FolderName.Create("Avatars").Value;

        var folder = Folder.Create(
            name: name,
            allowedFileType: FileType.Avatar);

        folder.Id.Value.ShouldNotBe(Guid.Empty);
        folder.Name.ShouldBe(name);
        folder.AllowedFileType.ShouldBe(FileType.Avatar);
        folder.ParentId.ShouldBeNull();
    }

    [Fact(DisplayName = "Child folder should inherit its allowed file type when parent exists")]
    public void CreateChild_Should_InheritAllowedFileTypeAndReferenceParent_When_ParentExists()
    {
        var parent = Folder.Create(
            name: FolderName.Create("Avatars").Value,
            allowedFileType: FileType.Avatar);
        var name = FolderName.Create("Players").Value;

        var child = parent.CreateChild(name);

        child.Id.ShouldNotBe(parent.Id);
        child.Name.ShouldBe(name);
        child.AllowedFileType.ShouldBe(parent.AllowedFileType);
        child.ParentId.ShouldBe(parent.Id);
        parent.ParentId.ShouldBeNull();
    }

    [Fact(DisplayName = "Nested folder should keep the allowed file type when parent is nested")]
    public void CreateChild_Should_KeepAllowedFileType_When_ParentIsNested()
    {
        var root = Folder.Create(
            name: FolderName.Create("Avatars").Value,
            allowedFileType: FileType.Avatar);
        var parent = root.CreateChild(FolderName.Create("Players").Value);

        var child = parent.CreateChild(FolderName.Create("Warriors").Value);

        child.ParentId.ShouldBe(parent.Id);
        child.ParentId.ShouldNotBe(root.Id);
        child.AllowedFileType.ShouldBe(root.AllowedFileType);
    }

    [Fact(DisplayName = "Rename should preserve folder identity and placement when folder is nested")]
    public void Rename_Should_PreserveIdentityAllowedFileTypeAndParent_When_FolderIsNested()
    {
        var parent = Folder.Create(
            name: FolderName.Create("Avatars").Value,
            allowedFileType: FileType.Avatar);
        var folder = parent.CreateChild(FolderName.Create("Players").Value);
        var id = folder.Id;
        var name = FolderName.Create("Heroes").Value;

        folder.Rename(name);

        folder.Id.ShouldBe(id);
        folder.Name.ShouldBe(name);
        folder.AllowedFileType.ShouldBe(parent.AllowedFileType);
        folder.ParentId.ShouldBe(parent.Id);
    }
}
