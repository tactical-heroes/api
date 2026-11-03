using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.CreatePersonal;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Services;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Folders.CreatePersonal;

public sealed class CreatePersonalFolderHandlerTests
{
    private readonly IFoldersRepository _folders = Substitute.For<IFoldersRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly Guid _userId = Guid.NewGuid();

    public CreatePersonalFolderHandlerTests()
    {
        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.UserId.Returns(_userId);
    }

    [Theory(DisplayName = "Personal folder handler should assign the current user when creation is permitted")]
    [InlineData("file-manager.personal-files.create.own")]
    [InlineData("file-manager.personal-files.create.all")]
    public async Task HandleAsync_Should_AssignCurrentUser_When_CreationIsPermitted(string permission)
    {
        _currentUser.HasPermission(permission).Returns(true);
        var command = new CreatePersonalFolderCommand(" Documents ", ParentId: null);

        var result = await CreateHandler().HandleAsync(command, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        _folders.Received(1).Add(Arg.Is<Folder>(folder => folder.Id.Value == result.Value
            && folder.Name.Value == "Documents"
            && folder.AllowedFileType == FileType.Personal
            && folder.UserId == UserId.Create(_userId).Value
            && folder.ParentId == null));
    }

    [Theory(DisplayName = "Personal folder handler should preserve ownership when creating a child")]
    [InlineData("file-manager.personal-files.create.own")]
    [InlineData("file-manager.personal-files.create.all")]
    public async Task HandleAsync_Should_PreserveOwnership_When_CreatingAChild(string permission)
    {
        _currentUser.HasPermission(permission).Returns(true);
        var parent = CreateParent(FileType.Personal, _userId);
        _folders.GetByIdAsync(parent.Id, Arg.Any<CancellationToken>()).Returns(parent);
        var command = new CreatePersonalFolderCommand("Child", parent.Id.Value);

        var result = await CreateHandler().HandleAsync(command, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        _folders.Received(1).Add(Arg.Is<Folder>(folder => folder.ParentId == parent.Id
            && folder.Name.Value == "Child"
            && folder.UserId == parent.UserId
            && folder.AllowedFileType == FileType.Personal));
        _folders.DidNotReceiveWithAnyArgs().Update(null!);
    }

    [Theory(
        DisplayName = "Personal folder handler should conceal the parent when all permissions cannot make it usable")]
    [InlineData("Personal")]
    [InlineData("Avatar")]
    public async Task HandleAsync_Should_ConcealParent_When_AllPermissionsCannotMakeItUsable(string type)
    {
        _currentUser.HasPermission("file-manager.personal-files.create.all").Returns(true);
        _currentUser.HasPermission("file-manager.avatars.create").Returns(true);
        var parent = CreateParent(FileType.FromName(type), type == "Personal" ? Guid.NewGuid() : null);
        _folders.GetByIdAsync(parent.Id, Arg.Any<CancellationToken>()).Returns(parent);
        var command = new CreatePersonalFolderCommand("Child", parent.Id.Value);

        var result = await CreateHandler().HandleAsync(command, TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(ErrorType.NotFound, "Folder was not found.");
        _folders.DidNotReceiveWithAnyArgs().Add(null!);
    }

    [Fact(DisplayName = "Personal folder handler should require an account when user id is unavailable")]
    public async Task HandleAsync_Should_RequireAccount_When_UserIdIsUnavailable()
    {
        _currentUser.UserId.Returns((Guid?)null);
        _currentUser.HasPermission("file-manager.personal-files.create.all").Returns(true);
        var command = new CreatePersonalFolderCommand("Documents", ParentId: null);

        var result = await CreateHandler().HandleAsync(command, TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(ErrorType.Unauthorized, "A user account is required.");
        _folders.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Personal folder handler should deny creation when only reading is permitted")]
    public async Task HandleAsync_Should_DenyCreation_When_OnlyReadingIsPermitted()
    {
        _currentUser.HasPermission("file-manager.personal-files.read.own").Returns(true);
        var command = new CreatePersonalFolderCommand("Documents", ParentId: null);

        var result = await CreateHandler().HandleAsync(command, TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(ErrorType.Forbidden, "Access to folders is denied.");
        _folders.DidNotReceiveWithAnyArgs().Add(null!);
    }

    [Theory(DisplayName = "Personal folder handler should refuse insertion when metadata is invalid")]
    [InlineData("../folder", false)]
    [InlineData("Documents", true)]
    public async Task HandleAsync_Should_RefuseInsertion_When_MetadataIsInvalid(string name, bool emptyParent)
    {
        _currentUser.HasPermission("file-manager.personal-files.create.own").Returns(true);
        var command = new CreatePersonalFolderCommand(name, emptyParent ? Guid.Empty : null);

        var result = await CreateHandler().HandleAsync(command, TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldAllBe(error => error.Type == ErrorType.Validation);
        _folders.ReceivedCalls().ShouldBeEmpty();
    }

    private static Folder CreateParent(FileType type, Guid? userId)
    {
        return Folder.Create(
            FolderName.Create("Parent").Value,
            type,
            UserId.Create(userId).Value).Value;
    }

    private CreatePersonalFolderHandler CreateHandler()
    {
        var policy = new FolderAccessPolicy(_currentUser, new FileAccessPolicy(_currentUser));
        return new CreatePersonalFolderHandler(new FolderCreationService(_folders, policy), _currentUser);
    }
}
