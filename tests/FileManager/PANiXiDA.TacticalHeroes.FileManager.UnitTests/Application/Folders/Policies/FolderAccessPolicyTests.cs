using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Folders.Policies;

public sealed class FolderAccessPolicyTests
{
    [Theory(DisplayName = "Folder access policy should check creation when type owner and permissions vary")]
    [InlineData("Personal", true, "file-manager.personal-files.create.own", true)]
    [InlineData("Personal", false, "file-manager.personal-files.create.own", false)]
    [InlineData("Personal", true, "file-manager.personal-files.create.all", true)]
    [InlineData("Personal", false, "file-manager.personal-files.create.all", true)]
    [InlineData("Personal", true, "file-manager.personal-files.read.all", false)]
    [InlineData("Personal", true, "file-manager.personal-files.update.own", false)]
    [InlineData("Personal", true, "file-manager.avatars.create", false)]
    [InlineData("Personal", true, "", false)]
    [InlineData("Avatar", false, "file-manager.avatars.create", true)]
    [InlineData("Avatar", false, "file-manager.avatars.read", false)]
    [InlineData("Avatar", false, "file-manager.personal-files.create.all", false)]
    [InlineData("Avatar", false, "", false)]
    public void CanCreate_Should_CheckCreation_When_TypeOwnerAndPermissionsVary(
        string type, bool own, string permission, bool allowed)
    {
        var userId = Guid.NewGuid();
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.IsAuthenticated.Returns(true);
        currentUser.UserId.Returns(userId);
        currentUser.HasPermission(permission).Returns(true);
        var folder = Folder.Create(
            FolderName.Create("Documents").Value,
            FileType.FromName(type),
            type == "Personal" ? UserId.Create(own ? userId : Guid.NewGuid()).Value : null).Value;
        var policy = new FolderAccessPolicy(currentUser, new FileAccessPolicy(currentUser));

        var result = policy.CanCreate(folder);

        result.ShouldBe(allowed);
    }

    [Theory(DisplayName = "Folder access policy should deny creation when caller is unauthenticated")]
    [InlineData("Personal", "file-manager.personal-files.create.all")]
    [InlineData("Avatar", "file-manager.avatars.create")]
    public void CanCreate_Should_DenyCreation_When_CallerIsUnauthenticated(string type, string permission)
    {
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.HasPermission(permission).Returns(true);
        var folder = Folder.Create(
            FolderName.Create("Documents").Value,
            FileType.FromName(type),
            type == "Personal" ? UserId.Create(Guid.NewGuid()).Value : null).Value;
        var policy = new FolderAccessPolicy(currentUser, new FileAccessPolicy(currentUser));

        var result = policy.CanCreate(folder);

        result.ShouldBeFalse();
    }
}
