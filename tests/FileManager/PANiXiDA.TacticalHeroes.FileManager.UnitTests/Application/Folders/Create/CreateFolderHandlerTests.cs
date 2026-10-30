using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Create;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Folders.Create;

public sealed class CreateFolderHandlerTests
{
    private readonly IFoldersRepository _folders = Substitute.For<IFoldersRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly Guid _userId = Guid.NewGuid();

    public CreateFolderHandlerTests()
    {
        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.UserId.Returns(_userId);
    }

    [Theory(DisplayName = "Create folder handler should add the root when creation is permitted")]
    [InlineData("Personal", "file-manager.personal-files.create.own", false)]
    [InlineData("Personal", "file-manager.personal-files.create.all", true)]
    [InlineData("Avatar", "file-manager.avatars.create", false)]
    public async Task HandleAsync_Should_AddRoot_When_CreationIsPermitted(
        string type, string permission, bool foreignOwner)
    {
        _currentUser.HasPermission(permission).Returns(true);
        Guid? ownerId = foreignOwner ? Guid.NewGuid() : null;
        var command = new CreateFolderCommand(" Documents ", type, ownerId, ParentId: null);

        var result = await CreateHandler().HandleAsync(command, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        _folders.Received(1).Add(Arg.Is<Folder>(folder => folder.Id.Value == result.Value
            && folder.Name.Value == "Documents"
            && folder.AllowedFileType.Name == type
            && folder.UserId == (type == "Personal" ? UserId.Create(ownerId ?? _userId).Value : null)
            && folder.ParentId == null));
        await _folders.DidNotReceiveWithAnyArgs().GetByIdAsync(default, TestContext.Current.CancellationToken);
    }

    [Theory(DisplayName = "Create folder handler should inherit parent metadata when a child is created")]
    [InlineData("Personal", false)]
    [InlineData("Personal", true)]
    [InlineData("Avatar", false)]
    public async Task HandleAsync_Should_InheritParentMetadata_When_AChildIsCreated(string type, bool foreignOwner)
    {
        var parent = Folder.Create(
            FolderName.Create("Parent").Value,
            FileType.FromName(type),
            type == "Personal" ? UserId.Create(foreignOwner ? Guid.NewGuid() : _userId).Value : null).Value;
        _folders.GetByIdAsync(parent.Id, Arg.Any<CancellationToken>()).Returns(parent);
        var permission = type == "Personal"
            ? foreignOwner ? "file-manager.personal-files.create.all" : "file-manager.personal-files.create.own"
            : "file-manager.avatars.create";
        _currentUser.HasPermission(permission).Returns(true);
        var command = new CreateFolderCommand("Child", type, UserId: null, parent.Id.Value);

        var result = await CreateHandler().HandleAsync(command, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        _folders.Received(1).Add(Arg.Is<Folder>(folder => folder.Id.Value == result.Value
            && folder.ParentId == parent.Id
            && folder.UserId == parent.UserId
            && folder.AllowedFileType == parent.AllowedFileType
            && folder.Name.Value == "Child"));
        parent.Name.Value.ShouldBe("Parent");
        parent.ParentId.ShouldBeNull();
    }

    [Theory(DisplayName = "Create folder handler should conceal the parent when parent is missing or forbidden")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_ConcealParent_When_ParentIsMissingOrForbidden(bool exists)
    {
        var parent = Folder.Create(
            FolderName.Create("Parent").Value,
            FileType.Personal,
            UserId.Create(Guid.NewGuid()).Value).Value;
        if (exists)
        {
            _folders.GetByIdAsync(parent.Id, Arg.Any<CancellationToken>()).Returns(parent);
        }
        _currentUser.HasPermission("file-manager.personal-files.create.own").Returns(true);
        var command = new CreateFolderCommand("Child", "Personal", UserId: null, parent.Id.Value);

        var result = await CreateHandler().HandleAsync(command, TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(ErrorType.NotFound, "Folder was not found.");
        _folders.DidNotReceiveWithAnyArgs().Add(null!);
    }

    [Theory(DisplayName = "Create folder handler should reject inconsistent metadata when child conflicts with parent")]
    [InlineData(true)]
    [InlineData(false)]
    public async Task HandleAsync_Should_RejectInconsistentMetadata_When_ChildConflictsWithParent(bool wrongType)
    {
        var parent = Folder.Create(
            FolderName.Create("Parent").Value,
            FileType.Personal,
            UserId.Create(_userId).Value).Value;
        _folders.GetByIdAsync(parent.Id, Arg.Any<CancellationToken>()).Returns(parent);
        _currentUser.HasPermission("file-manager.personal-files.create.all").Returns(true);
        var command = new CreateFolderCommand(
            "Child", wrongType ? "Avatar" : "Personal", wrongType ? null : Guid.NewGuid(), parent.Id.Value);

        var result = await CreateHandler().HandleAsync(command, TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldAllBe(error => error.Type == ErrorType.Validation);
        _folders.DidNotReceiveWithAnyArgs().Add(null!);
    }

    [Fact(DisplayName = "Create folder handler should refuse insertion when root creation is forbidden")]
    public async Task HandleAsync_Should_RefuseInsertion_When_RootCreationIsForbidden()
    {
        var command = new CreateFolderCommand("Documents", "Personal", UserId: null, ParentId: null);

        var result = await CreateHandler().HandleAsync(command, TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(ErrorType.Forbidden, "Access to folders is denied.");
        _folders.DidNotReceiveWithAnyArgs().Add(null!);
    }

    private CreateFolderHandler CreateHandler()
    {
        return new CreateFolderHandler(
            _folders,
            new FolderAccessPolicy(_currentUser, new FileAccessPolicy(_currentUser)),
            _currentUser);
    }
}
