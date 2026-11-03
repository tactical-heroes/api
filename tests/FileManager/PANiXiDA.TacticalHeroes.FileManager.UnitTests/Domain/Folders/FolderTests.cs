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
            allowedFileType: FileType.Avatar,
            userId: null).Value;

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
            result.ShouldHaveSingleError(
                ErrorType.Validation,
                owned
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
            allowedFileType: FileType.Avatar,
            userId: null).Value;
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
            allowedFileType: FileType.Avatar,
            userId: null).Value;
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

    [Theory(DisplayName = "Move should preserve folder metadata when parent is compatible")]
    [InlineData("Personal")]
    [InlineData("Avatar")]
    public void MoveTo_Should_PreserveMetadata_When_ParentIsCompatible(string type)
    {
        var owner = type == "Personal" ? UserId.Create(Guid.NewGuid()).Value : (UserId?)null;
        var folder = Folder.Create(FolderName.Create("Documents").Value, FileType.FromName(type), owner).Value;
        var parent = Folder.Create(FolderName.Create("Target").Value, folder.AllowedFileType, owner).Value;
        var id = folder.Id;
        var name = folder.Name;

        var result = folder.MoveTo(parent);

        result.IsSuccess.ShouldBeTrue();
        folder.ParentId.ShouldBe(parent.Id);
        folder.Id.ShouldBe(id);
        folder.Name.ShouldBe(name);
        folder.AllowedFileType.ShouldBe(parent.AllowedFileType);
        folder.UserId.ShouldBe(owner);
        parent.ParentId.ShouldBeNull();
    }

    [Fact(DisplayName = "Move should detach the folder when parent is null")]
    public void MoveTo_Should_DetachFolder_When_ParentIsNull()
    {
        var parent = Folder.Create(FolderName.Create("Root").Value, FileType.Avatar, userId: null).Value;
        var folder = parent.CreateChild(FolderName.Create("Child").Value);

        var result = folder.MoveTo(parent: null);

        result.IsSuccess.ShouldBeTrue();
        folder.ParentId.ShouldBeNull();
        folder.Name.Value.ShouldBe("Child");
        folder.AllowedFileType.ShouldBe(FileType.Avatar);
    }

    [Fact(DisplayName = "Move should reject the parent when parent is the folder itself")]
    public void MoveTo_Should_RejectParent_When_ParentIsTheFolderItself()
    {
        var folder = Folder.Create(FolderName.Create("Root").Value, FileType.Avatar, userId: null).Value;

        var result = folder.MoveTo(folder);

        result.ShouldHaveSingleError(ErrorType.Validation, "A folder cannot be its own parent.")
            .ShouldHaveField(nameof(Folder.ParentId));
        folder.ParentId.ShouldBeNull();
    }

    [Theory(DisplayName = "Move should preserve placement when parent type or owner is incompatible")]
    [InlineData(false)]
    [InlineData(true)]
    public void MoveTo_Should_PreservePlacement_When_ParentTypeOrOwnerIsIncompatible(bool wrongOwner)
    {
        var owner = UserId.Create(Guid.NewGuid()).Value;
        var root = Folder.Create(FolderName.Create("Root").Value, FileType.Personal, owner).Value;
        var folder = root.CreateChild(FolderName.Create("Child").Value);
        var parent = Folder.Create(
            FolderName.Create("Target").Value,
            wrongOwner ? FileType.Personal : FileType.Avatar,
            wrongOwner ? UserId.Create(Guid.NewGuid()).Value : null).Value;

        var result = folder.MoveTo(parent);

        result.ShouldHaveSingleError(
            ErrorType.Validation,
            wrongOwner
                ? "Folder and parent user ids must match."
                : "Folder and parent allowed file types must match.")
            .ShouldHaveField(nameof(Folder.ParentId));
        folder.ParentId.ShouldBe(root.Id);
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
