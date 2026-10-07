using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Domain.Folders;

public sealed class FolderTests
{
    [Fact(DisplayName = "Folder should have no parent when created as a root")]
    public void Create_Should_ReturnRootFolder_When_TypeIsKnown()
    {
        var name = FolderName.Create("Avatars").Value;

        var folder = Folder.Create(
            name: name,
            type: FileType.Avatar);

        folder.Id.Value.ShouldNotBe(Guid.Empty);
        folder.Name.ShouldBe(name);
        folder.Type.ShouldBe(FileType.Avatar);
        folder.ParentId.ShouldBeNull();
    }

    [Fact(DisplayName = "Child folder should inherit its type when created under a parent")]
    public void CreateChild_Should_InheritTypeAndReferenceParent_When_ParentExists()
    {
        var parent = Folder.Create(
            name: FolderName.Create("Avatars").Value,
            type: FileType.Avatar);
        var name = FolderName.Create("Players").Value;

        var child = parent.CreateChild(name);

        child.Id.ShouldNotBe(parent.Id);
        child.Name.ShouldBe(name);
        child.Type.ShouldBe(parent.Type);
        child.ParentId.ShouldBe(parent.Id);
        parent.ParentId.ShouldBeNull();
    }

    [Fact(DisplayName = "Nested folder should keep the branch type when created below another child")]
    public void CreateChild_Should_KeepBranchType_When_ParentIsNested()
    {
        var root = Folder.Create(
            name: FolderName.Create("Avatars").Value,
            type: FileType.Avatar);
        var parent = root.CreateChild(FolderName.Create("Players").Value);

        var child = parent.CreateChild(FolderName.Create("Warriors").Value);

        child.ParentId.ShouldBe(parent.Id);
        child.ParentId.ShouldNotBe(root.Id);
        child.Type.ShouldBe(root.Type);
    }

    [Fact(DisplayName = "Rename should preserve folder identity and placement when a nested folder is renamed")]
    public void Rename_Should_PreserveIdentityTypeAndParent_When_FolderIsNested()
    {
        var parent = Folder.Create(
            name: FolderName.Create("Avatars").Value,
            type: FileType.Avatar);
        var folder = parent.CreateChild(FolderName.Create("Players").Value);
        var id = folder.Id;
        var name = FolderName.Create("Heroes").Value;

        folder.Rename(name);

        folder.Id.ShouldBe(id);
        folder.Name.ShouldBe(name);
        folder.Type.ShouldBe(parent.Type);
        folder.ParentId.ShouldBe(parent.Id);
    }
}
