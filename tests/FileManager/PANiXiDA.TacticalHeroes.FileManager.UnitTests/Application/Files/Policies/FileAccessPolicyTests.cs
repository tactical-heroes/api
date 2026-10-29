using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Common.Filters;
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

    [Theory(DisplayName = "File access policy should match aggregate and filter decisions when permissions vary")]
    [InlineData("Personal", "file-manager.personal-files.read.own", true, true)]
    [InlineData("Personal", "file-manager.personal-files.read.own", false, false)]
    [InlineData("Personal", "file-manager.personal-files.read.all", false, true)]
    [InlineData("Avatar", "file-manager.avatars.read", false, true)]
    [InlineData("Avatar", "file-manager.personal-files.read.all", false, false)]
    [InlineData("Personal", "file-manager.avatars.read", true, false)]
    public void CanRead_Should_MatchAggregateAndFilterDecisions_When_PermissionsVary(
        string type, string permission, bool own, bool expected)
    {
        var userId = Guid.NewGuid();
        var currentUser = CreateCurrentUser(userId);
        currentUser.HasPermission(permission).Returns(true);
        var file = CreateFile(type, own ? userId : Guid.NewGuid());
        var filter = new FilesFilter(Type: file.Type.Name, UserId: file.UserId?.Value);
        var policy = new FileAccessPolicy(currentUser);

        var aggregateAllowed = policy.CanRead(file);
        var filterAllowed = policy.CanRead(filter);

        aggregateAllowed.ShouldBe(expected);
        filterAllowed.ShouldBe(expected);
    }

    [Theory(DisplayName = "File access policy should require the full selection when type is omitted")]
    [InlineData(true, true, false, true)]
    [InlineData(true, false, false, false)]
    [InlineData(false, true, false, false)]
    [InlineData(false, true, true, true)]
    public void CanRead_Should_RequireFullSelection_When_TypeIsOmitted(bool all, bool avatars, bool own, bool expected)
    {
        var userId = Guid.NewGuid();
        var currentUser = CreateCurrentUser(userId);
        currentUser.HasPermission(Permissions.FileManager.PersonalFiles.ReadAll.Code).Returns(all);
        currentUser.HasPermission(Permissions.FileManager.PersonalFiles.ReadOwn.Code).Returns(own);
        currentUser.HasPermission(Permissions.FileManager.Avatars.Read.Code).Returns(avatars);
        var filter = new FilesFilter(UserId: own ? userId : null);

        var allowed = new FileAccessPolicy(currentUser).CanRead(filter);

        allowed.ShouldBe(expected);
    }

    [Theory(DisplayName = "File access policy should authorize and normalize the filter when type and owner vary")]
    [InlineData("Personal", "own", false, true, false, true, "Personal", "own")]
    [InlineData("Personal", "foreign", false, true, false, false, null, "none")]
    [InlineData("Personal", "none", false, true, false, true, "Personal", "own")]
    [InlineData("Personal", "foreign", true, false, false, true, "Personal", "foreign")]
    [InlineData("Personal", "foreign", true, true, false, true, "Personal", "foreign")]
    [InlineData("Personal", "none", true, true, false, true, "Personal", "none")]
    [InlineData(" Personal ", "none", false, true, false, true, " Personal ", "own")]
    [InlineData("Avatar", "none", false, false, true, true, "Avatar", "none")]
    [InlineData(null, "own", false, true, true, true, null, "own")]
    [InlineData(null, "own", false, true, false, true, "Personal", "own")]
    [InlineData(null, "foreign", false, true, true, false, null, "none")]
    [InlineData("Unknown", "none", true, true, true, false, null, "none")]
    [InlineData("", "none", true, true, true, false, null, "none")]
    [InlineData(null, "none", false, false, true, true, "Avatar", "none")]
    [InlineData(null, "none", false, true, false, true, "Personal", "own")]
    [InlineData(null, "none", false, true, true, true, null, "own")]
    [InlineData(null, "none", true, false, false, true, "Personal", "none")]
    [InlineData(null, "none", true, true, true, true, null, "none")]
    [InlineData(null, "none", false, false, false, false, null, "none")]
    public void ApplyReadRestrictions_Should_AuthorizeAndNormalizeFilter_When_TypeAndOwnerVary(
        string? type,
        string owner,
        bool all,
        bool own,
        bool avatars,
        bool expected,
        string? expectedType,
        string expectedOwner)
    {
        var userId = Guid.NewGuid();
        var foreignUserId = Guid.NewGuid();
        var currentUser = CreateCurrentUser(userId);
        currentUser.HasPermission(Permissions.FileManager.PersonalFiles.ReadAll.Code).Returns(all);
        currentUser.HasPermission(Permissions.FileManager.PersonalFiles.ReadOwn.Code).Returns(own);
        currentUser.HasPermission(Permissions.FileManager.Avatars.Read.Code).Returns(avatars);
        var ownerId = owner == "own" ? userId : owner == "foreign" ? foreignUserId : (Guid?)null;
        var filter = new FilesFilter(Type: type, Search: "report", UserId: ownerId);

        var result = new FileAccessPolicy(currentUser).ApplyReadRestrictions(filter);

        result.IsSuccess.ShouldBe(expected);
        filter.Type.ShouldBe(type);
        filter.UserId.ShouldBe(ownerId);
        if (expected)
        {
            var expectedOwnerId = expectedOwner == "own"
                ? userId : expectedOwner == "foreign" ? foreignUserId : (Guid?)null;
            result.Value.ShouldBe(new FilesFilter(Type: expectedType, Search: "report", UserId: expectedOwnerId));
        }
        else
        {
            result.ShouldHaveSingleError(ErrorType.Forbidden, "Access to files is denied.");
        }
    }

    [Theory(DisplayName = "File access policy should reject caller when authentication or user id is absent")]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void ApplyReadRestrictions_Should_RejectCaller_When_AuthenticationOrUserIdIsAbsent(
        bool authenticated,
        bool hasUserId)
    {
        var currentUser = CreateCurrentUser(hasUserId ? Guid.NewGuid() : null);
        currentUser.IsAuthenticated.Returns(authenticated);
        currentUser.HasPermission(Arg.Any<string>()).Returns(true);

        var result = new FileAccessPolicy(currentUser).ApplyReadRestrictions(new FilesFilter());

        result.ShouldHaveSingleError(ErrorType.Unauthorized, "A user account is required.");
        currentUser.DidNotReceive().HasPermission(Arg.Any<string>());
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
