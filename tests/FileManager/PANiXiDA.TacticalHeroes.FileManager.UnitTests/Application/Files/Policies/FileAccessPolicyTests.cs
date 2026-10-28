using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Files.Policies;

public sealed class FileAccessPolicyTests
{
    [Theory(DisplayName = "File access policy should allow reading when the matching permission is granted")]
    [InlineData("Personal", "file-manager.personal-files.read.own")]
    [InlineData("Avatar", "file-manager.avatars.read")]
    public void CanRead_Should_AllowReading_When_TheMatchingPermissionIsGranted(string typeName, string permission)
    {
        var userId = Guid.NewGuid();
        var currentUser = CreateCurrentUser(userId);
        currentUser.HasPermission(permission).Returns(true);
        var file = CreateFile(typeName, userId);

        var allowed = new FileAccessPolicy(currentUser).CanRead(file);

        allowed.ShouldBeTrue();
        currentUser.Received(1).HasPermission(permission);
    }

    [Theory(DisplayName = "File access policy should deny reading when permission belongs to another file type")]
    [InlineData("Personal", "file-manager.avatars.read")]
    [InlineData("Avatar", "file-manager.personal-files.read.own")]
    public void CanRead_Should_DenyReading_When_PermissionBelongsToAnotherFileType(string typeName, string permission)
    {
        var userId = Guid.NewGuid();
        var currentUser = CreateCurrentUser(userId);
        currentUser.HasPermission(permission).Returns(true);
        var file = CreateFile(typeName, userId);

        var allowed = new FileAccessPolicy(currentUser).CanRead(file);

        allowed.ShouldBeFalse();
    }

    [Theory(DisplayName = "File access policy should deny reading when no permission is granted")]
    [InlineData("Personal")]
    [InlineData("Avatar")]
    public void CanRead_Should_DenyReading_When_NoPermissionIsGranted(string typeName)
    {
        var userId = Guid.NewGuid();
        var currentUser = CreateCurrentUser(userId);
        var file = CreateFile(typeName, userId);

        var allowed = new FileAccessPolicy(currentUser).CanRead(file);

        allowed.ShouldBeFalse();
    }

    [Fact(DisplayName = "File access policy should deny personal files when another user owns the file")]
    public void CanRead_Should_DenyPersonalFiles_When_AnotherUserOwnsTheFile()
    {
        var currentUser = CreateCurrentUser(Guid.NewGuid());
        currentUser.HasPermission(Permissions.FileManager.PersonalFiles.ReadOwn.Code).Returns(true);
        var file = CreateFile("Personal", Guid.NewGuid());

        var allowed = new FileAccessPolicy(currentUser).CanRead(file);

        allowed.ShouldBeFalse();
    }

    [Theory(DisplayName = "File access policy should allow personal files when read all is granted")]
    [InlineData(false)]
    [InlineData(true)]
    public void CanRead_Should_AllowPersonalFiles_When_ReadAllIsGranted(bool ownsFile)
    {
        var userId = Guid.NewGuid();
        var currentUser = CreateCurrentUser(userId);
        currentUser.HasPermission(Permissions.FileManager.PersonalFiles.ReadAll.Code).Returns(true);
        var file = CreateFile("Personal", ownsFile ? userId : Guid.NewGuid());

        var allowed = new FileAccessPolicy(currentUser).CanRead(file);

        allowed.ShouldBeTrue();
    }

    [Theory(DisplayName = "File access policy should deny personal files when permission does not grant reading")]
    [InlineData("file-manager.personal-files.create.own")]
    [InlineData("file-manager.personal-files.create.all")]
    [InlineData("file-manager.personal-files.update.own")]
    [InlineData("file-manager.personal-files.update.all")]
    [InlineData("file-manager.personal-files.delete.own")]
    [InlineData("file-manager.personal-files.delete.all")]
    [InlineData("file-manager.personal-files.read")]
    public void CanRead_Should_DenyPersonalFiles_When_PermissionDoesNotGrantReading(string permission)
    {
        var userId = Guid.NewGuid();
        var currentUser = CreateCurrentUser(userId);
        currentUser.HasPermission(permission).Returns(true);
        var file = CreateFile("Personal", userId);

        var allowed = new FileAccessPolicy(currentUser).CanRead(file);

        allowed.ShouldBeFalse();
    }

    [Theory(DisplayName = "File access policy should deny personal files when caller has no valid user id")]
    [InlineData(false)]
    [InlineData(true)]
    public void CanRead_Should_DenyPersonalFiles_When_CallerHasNoValidUserId(bool emptyUserId)
    {
        var currentUser = CreateCurrentUser(emptyUserId ? Guid.Empty : null);
        currentUser.HasPermission(Permissions.FileManager.PersonalFiles.ReadOwn.Code).Returns(true);
        var file = CreateFile("Personal", Guid.NewGuid());

        var allowed = new FileAccessPolicy(currentUser).CanRead(file);

        allowed.ShouldBeFalse();
    }

    [Theory(DisplayName = "File access policy should deny reading when caller is unauthenticated")]
    [InlineData("Personal", "file-manager.personal-files.read.own")]
    [InlineData("Personal", "file-manager.personal-files.read.all")]
    [InlineData("Avatar", "file-manager.avatars.read")]
    public void CanRead_Should_DenyReading_When_CallerIsUnauthenticated(string typeName, string permission)
    {
        var userId = Guid.NewGuid();
        var currentUser = CreateCurrentUser(userId);
        currentUser.IsAuthenticated.Returns(false);
        currentUser.HasPermission(permission).Returns(true);
        var file = CreateFile(typeName, userId);

        var allowed = new FileAccessPolicy(currentUser).CanRead(file);

        allowed.ShouldBeFalse();
        currentUser.DidNotReceive().HasPermission(Arg.Any<string>());
    }

    [Theory(DisplayName = "File access policy should restrict ownership when personal read scope varies")]
    [InlineData(false)]
    [InlineData(true)]
    public void GetReadAccess_Should_RestrictOwnership_When_PersonalReadScopeVaries(bool all)
    {
        var userId = Guid.NewGuid();
        var currentUser = CreateCurrentUser(userId);
        currentUser.HasPermission(Permissions.FileManager.PersonalFiles.ReadOwn.Code).Returns(true);
        currentUser.HasPermission(Permissions.FileManager.PersonalFiles.ReadAll.Code).Returns(all);

        var result = new FileAccessPolicy(currentUser).GetReadAccess(FileType.Personal);

        result.IsSuccess.ShouldBeTrue();
        result.Value.UserId.ShouldBe(all ? null : userId);
    }

    private static ICurrentUser CreateCurrentUser(Guid? userId)
    {
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.IsAuthenticated.Returns(true);
        currentUser.UserId.Returns(userId);
        return currentUser;
    }

    private static File CreateFile(string typeName, Guid userId)
    {
        var type = FileType.FromName(typeName);
        return File.Create(
            FileName.Create("file.txt").Value,
            type,
            type == FileType.Personal ? UserId.Create(userId).Value : null).Value;
    }
}
