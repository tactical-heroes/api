using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Domain.Folders;

public sealed class FolderTests
{
    [Fact(DisplayName = "Folder should have no parent when allowed file type is known")]
    public void Create_Should_ReturnRootFolder_When_AllowedFileTypeIsKnown()
    {
        var name = FolderName.Create("Avatars").Value;

        var folder = Folder.Create(
            name: name,
            allowedFileType: FileType.Avatar).Value;

        folder.Id.Value.ShouldNotBe(Guid.Empty);
        folder.Name.ShouldBe(name);
        folder.AllowedFileType.ShouldBe(FileType.Avatar);
        folder.UserId.ShouldBeNull();
        folder.ParentId.ShouldBeNull();
    }

    [Theory(DisplayName = "Folder should require a user id only for personal files when ownership varies")]
    [InlineData("Avatar", false, true)]
    [InlineData("Avatar", true, false)]
    [InlineData("Personal", false, false)]
    [InlineData("Personal", true, true)]
    public void Create_Should_RequireUserIdOnlyForPersonalFiles_When_OwnershipVaries(
        string typeName, bool owned, bool valid)
    {
        UserId? userId = owned ? UserId.Create(Guid.CreateVersion7()).Value : null;

        var result = Folder.Create(
            name: FolderName.Create("Images").Value,
            allowedFileType: FileType.FromName(typeName),
            userId: userId);

        result.IsSuccess.ShouldBe(valid);
        if (valid)
        {
            result.Value.AllowedFileType.Name.ShouldBe(typeName);
            result.Value.UserId.ShouldBe(userId);
            result.Value.ParentId.ShouldBeNull();
        }
        else
        {
            result.ShouldHaveSingleError(ErrorType.Validation, owned
                    ? "Only personal folders can have a user id."
                    : "Personal folders require a user id.")
                .ShouldHaveField(nameof(Folder.UserId));
        }
    }

    [Fact(DisplayName = "Child folder should inherit its allowed file type when parent exists")]
    public void CreateChild_Should_InheritAllowedFileTypeAndReferenceParent_When_ParentExists()
    {
        var parent = Folder.Create(
            name: FolderName.Create("Avatars").Value,
            allowedFileType: FileType.Avatar).Value;
        var name = FolderName.Create("Players").Value;

        var child = parent.CreateChild(name);

        child.Id.ShouldNotBe(parent.Id);
        child.Name.ShouldBe(name);
        child.AllowedFileType.ShouldBe(parent.AllowedFileType);
        child.UserId.ShouldBeNull();
        child.ParentId.ShouldBe(parent.Id);
        parent.ParentId.ShouldBeNull();
    }

    [Fact(DisplayName = "Nested folder should keep the allowed file type when parent is nested")]
    public void CreateChild_Should_KeepAllowedFileType_When_ParentIsNested()
    {
        var root = Folder.Create(
            name: FolderName.Create("Avatars").Value,
            allowedFileType: FileType.Avatar).Value;
        var parent = root.CreateChild(FolderName.Create("Players").Value);

        var child = parent.CreateChild(FolderName.Create("Warriors").Value);

        child.ParentId.ShouldBe(parent.Id);
        child.ParentId.ShouldNotBe(root.Id);
        child.AllowedFileType.ShouldBe(root.AllowedFileType);
    }

    [Theory(DisplayName = "Child folder should inherit its owner when depth varies")]
    [InlineData(1)]
    [InlineData(3)]
    public void CreateChild_Should_InheritUserId_When_DepthVaries(int depth)
    {
        var userId = UserId.Create(Guid.CreateVersion7()).Value;
        var folder = Folder.Create(
            name: FolderName.Create("Personal").Value,
            allowedFileType: FileType.Personal,
            userId: userId).Value;

        for (var level = 0; level < depth; level++)
        {
            folder = folder.CreateChild(FolderName.Create($"Level {level}").Value);
        }

        folder.UserId.ShouldBe(userId);
        folder.AllowedFileType.ShouldBe(FileType.Personal);
    }

    [Fact(DisplayName = "Rename should preserve folder identity and placement when folder is nested")]
    public void Rename_Should_PreserveIdentityAllowedFileTypeAndParent_When_FolderIsNested()
    {
        var userId = UserId.Create(Guid.CreateVersion7()).Value;
        var parent = Folder.Create(
            name: FolderName.Create("Personal").Value,
            allowedFileType: FileType.Personal,
            userId: userId).Value;
        var folder = parent.CreateChild(FolderName.Create("Players").Value);
        var id = folder.Id;
        var name = FolderName.Create("Heroes").Value;

        folder.Rename(name);

        folder.Id.ShouldBe(id);
        folder.Name.ShouldBe(name);
        folder.AllowedFileType.ShouldBe(parent.AllowedFileType);
        folder.UserId.ShouldBe(userId);
        folder.ParentId.ShouldBe(parent.Id);
    }
}
