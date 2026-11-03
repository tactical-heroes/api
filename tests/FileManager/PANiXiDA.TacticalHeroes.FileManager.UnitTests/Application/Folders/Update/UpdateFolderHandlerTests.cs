using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Update;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Folders.Update;

public sealed class UpdateFolderHandlerTests
{
    private readonly IFoldersRepository _folders = Substitute.For<IFoldersRepository>();
    private readonly IFoldersReadRepository _read = Substitute.For<IFoldersReadRepository>();
    private readonly IFolderHierarchyLock _hierarchyLock = Substitute.For<IFolderHierarchyLock>();
    private readonly ICurrentUser _user = Substitute.For<ICurrentUser>();
    private readonly Guid _userId = Guid.NewGuid();

    public UpdateFolderHandlerTests()
    {
        _user.IsAuthenticated.Returns(true);
        _user.UserId.Returns(_userId);
        _user.HasPermission("file-manager.personal-files.update.all").Returns(true);
        _user.HasPermission("file-manager.avatars.update").Returns(true);
    }

    [Theory(DisplayName = "Update folder handler should replace metadata when scope transition is permitted")]
    [InlineData("Personal")]
    [InlineData("Avatar")]
    public async Task HandleAsync_Should_ReplaceMetadata_When_ScopeTransitionIsPermitted(string type)
    {
        var folder = CreateFolder(_userId);
        var ownerId = type == "Personal" ? Guid.NewGuid() : (Guid?)null;
        var parent = Folder.Create(
            FolderName.Create("Parent").Value, FileType.FromName(type), UserId.Create(ownerId).Value).Value;
        _folders.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);
        _folders.GetByIdAsync(parent.Id, Arg.Any<CancellationToken>()).Returns(parent);

        var result = await CreateHandler().HandleAsync(
            new UpdateFolderCommand(folder.Id.Value, " After ", type, ownerId, parent.Id.Value),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        folder.Name.Value.ShouldBe("After");
        folder.AllowedFileType.ShouldBe(FileType.FromName(type));
        (folder.UserId?.Value).ShouldBe(ownerId);
        folder.ParentId.ShouldBe(parent.Id);
        _folders.Received(1).Update(Arg.Any<Folder>());
        _folders.Received(1).Update(folder);
        await _read.Received(1).HasContentsAsync(folder.Id.Value, TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Update folder handler should avoid inspecting contents when scope is unchanged")]
    public async Task HandleAsync_Should_AvoidInspectingContents_When_ScopeIsUnchanged()
    {
        var folder = CreateFolder(_userId);
        _folders.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);
        _read.HasContentsAsync(folder.Id.Value, Arg.Any<CancellationToken>()).Returns(true);

        var result = await CreateHandler().HandleAsync(
            new UpdateFolderCommand(folder.Id.Value, "After", "Personal", _userId),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        folder.Name.Value.ShouldBe("After");
        await _read.DidNotReceiveWithAnyArgs().HasContentsAsync(default, TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Update folder handler should preserve all metadata when scope changes with active contents")]
    public async Task HandleAsync_Should_PreserveAllMetadata_When_ScopeChangesWithActiveContents()
    {
        var folder = CreateFolder(_userId);
        _folders.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);
        _read.HasContentsAsync(folder.Id.Value, Arg.Any<CancellationToken>()).Returns(true);

        var result = await CreateHandler().HandleAsync(
            new UpdateFolderCommand(folder.Id.Value, "After", "Avatar"), TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(
            ErrorType.Conflict, "Folder type and owner can only be changed when the folder is empty.");
        folder.Name.Value.ShouldBe("Before");
        folder.AllowedFileType.ShouldBe(FileType.Personal);
        (folder.UserId?.Value).ShouldBe(_userId);
        _folders.DidNotReceiveWithAnyArgs().Update(null!);
    }

    [Fact(DisplayName = "Update folder handler should deny transfer when only own permission is granted")]
    public async Task HandleAsync_Should_DenyTransfer_When_OnlyOwnPermissionIsGranted()
    {
        var folder = CreateFolder(_userId);
        _folders.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);
        _user.HasPermission("file-manager.personal-files.update.all").Returns(false);
        _user.HasPermission("file-manager.personal-files.update.own").Returns(true);

        var result = await CreateHandler().HandleAsync(
            new UpdateFolderCommand(folder.Id.Value, "After", "Personal", Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(
            ErrorType.Forbidden, "Access to the requested folder type or owner is denied.");
        folder.Name.Value.ShouldBe("Before");
        _folders.DidNotReceiveWithAnyArgs().Update(null!);
    }

    [Fact(DisplayName = "Update folder handler should preserve the name when destination descends from the source")]
    public async Task HandleAsync_Should_PreserveName_When_DestinationDescendsFromTheSource()
    {
        var folder = CreateFolder(_userId);
        var parent = folder.CreateChild(FolderName.Create("Child").Value);
        _folders.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);
        _folders.GetByIdAsync(parent.Id, Arg.Any<CancellationToken>()).Returns(parent);
        _read.HasAncestorAsync(parent.Id.Value, folder.Id.Value, Arg.Any<CancellationToken>()).Returns(true);

        var result = await CreateHandler().HandleAsync(
            new UpdateFolderCommand(folder.Id.Value, "After", "Personal", _userId, parent.Id.Value),
            TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(
            ErrorType.Validation, "A folder cannot be moved into one of its descendants.");
        folder.Name.Value.ShouldBe("Before");
        folder.ParentId.ShouldBeNull();
        _folders.DidNotReceiveWithAnyArgs().Update(null!);
    }

    [Theory(DisplayName = "Update folder handler should avoid persistence when input or source is unavailable")]
    [InlineData("invalid")]
    [InlineData("missing")]
    [InlineData("forbidden")]
    public async Task HandleAsync_Should_AvoidPersistence_When_InputOrSourceIsUnavailable(string kind)
    {
        var folder = CreateFolder(_userId);
        if (kind != "missing")
        {
            _folders.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);
        }
        if (kind == "forbidden")
        {
            _user.HasPermission("file-manager.personal-files.update.all").Returns(false);
        }

        var result = await CreateHandler().HandleAsync(
            new UpdateFolderCommand(folder.Id.Value, kind == "invalid" ? "../folder" : "After", "Personal", _userId),
            TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        _folders.DidNotReceiveWithAnyArgs().Update(null!);
        if (kind == "invalid")
        {
            _hierarchyLock.ReceivedCalls().ShouldBeEmpty();
        }
    }

    private static Folder CreateFolder(Guid ownerId)
    {
        return Folder.Create(
            FolderName.Create("Before").Value, FileType.Personal, UserId.Create(ownerId).Value).Value;
    }

    private UpdateFolderHandler CreateHandler()
    {
        return new UpdateFolderHandler(
            _folders, _read, _hierarchyLock, new FolderAccessPolicy(_user, new FileAccessPolicy(_user)));
    }
}
