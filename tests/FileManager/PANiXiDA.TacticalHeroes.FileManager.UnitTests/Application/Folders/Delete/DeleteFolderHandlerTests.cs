using NSubstitute.ExceptionExtensions;

using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.Core.Application.Messaging.EventBus;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Delete;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Events;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Folders.Delete;

public sealed class DeleteFolderHandlerTests
{
    private readonly IFoldersRepository _folders = Substitute.For<IFoldersRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IEventBus _eventBus = Substitute.For<IEventBus>();
    private readonly Guid _userId = Guid.NewGuid();

    public DeleteFolderHandlerTests()
    {
        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.UserId.Returns(_userId);
    }

    [Theory(DisplayName = "Delete folder handler should apply the policy when type owner and permissions vary")]
    [InlineData("Personal", true, "file-manager.personal-files.delete.own", true)]
    [InlineData("Personal", false, "file-manager.personal-files.delete.own", false)]
    [InlineData("Personal", false, "file-manager.personal-files.delete.all", true)]
    [InlineData("Personal", true, "file-manager.avatars.delete", false)]
    [InlineData("Personal", true, "file-manager.personal-files.read.all", false)]
    [InlineData("Personal", true, "", false)]
    [InlineData("Avatar", false, "file-manager.avatars.delete", true)]
    [InlineData("Avatar", false, "file-manager.personal-files.delete.all", false)]
    [InlineData("Avatar", false, "file-manager.avatars.read", false)]
    public async Task HandleAsync_Should_ApplyPolicy_When_TypeOwnerAndPermissionsVary(
        string type, bool own, string permission, bool allowed)
    {
        _currentUser.HasPermission(permission).Returns(true);
        var folder = Folder.Create(
            FolderName.Create("Documents").Value,
            FileType.FromName(type),
            type == "Personal" ? UserId.Create(own ? _userId : Guid.NewGuid()).Value : null).Value;
        _folders.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);

        var result = await CreateHandler().HandleAsync(
            new DeleteFolderCommand(folder.Id.Value), TestContext.Current.CancellationToken);

        if (allowed)
        {
            result.IsSuccess.ShouldBeTrue();
            _folders.Received(1).Delete(folder);
            await _eventBus.Received(1).PublishAsync(
                Arg.Is<FolderDeleted>(e => e.FolderId == folder.Id.Value), TestContext.Current.CancellationToken);
        }
        else
        {
            result.ShouldHaveSingleError(ErrorType.NotFound, "Folder was not found.");
            _folders.DidNotReceiveWithAnyArgs().Delete(null!);
            _eventBus.ReceivedCalls().ShouldBeEmpty();
        }
    }

    [Fact(DisplayName = "Delete folder handler should publish nothing when folder is missing")]
    public async Task HandleAsync_Should_PublishNothing_When_FolderIsMissing()
    {
        var command = new DeleteFolderCommand(Guid.NewGuid());

        var result = await CreateHandler().HandleAsync(command, TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(ErrorType.NotFound, "Folder was not found.");
        _folders.DidNotReceiveWithAnyArgs().Delete(null!);
        _eventBus.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Delete folder handler should reject input when id is empty")]
    public async Task HandleAsync_Should_RejectInput_When_IdIsEmpty()
    {
        var command = new DeleteFolderCommand(Guid.Empty);

        var result = await CreateHandler().HandleAsync(command, TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        _folders.ReceivedCalls().ShouldBeEmpty();
        _eventBus.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Delete folder handler should propagate failure when event publication fails")]
    public async Task HandleAsync_Should_PropagateFailure_When_EventPublicationFails()
    {
        var folder = Folder.Create(
            FolderName.Create("Documents").Value, FileType.Personal, UserId.Create(_userId).Value).Value;
        _folders.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);
        _currentUser.HasPermission("file-manager.personal-files.delete.own").Returns(true);
        _eventBus.PublishAsync(Arg.Any<FolderDeleted>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new IOException("Outbox unavailable."));

        var exception = await Record.ExceptionAsync(() => CreateHandler().HandleAsync(
            new DeleteFolderCommand(folder.Id.Value), TestContext.Current.CancellationToken));

        exception.ShouldBeOfType<IOException>();
    }

    private DeleteFolderHandler CreateHandler()
    {
        return new DeleteFolderHandler(
            _folders,
            new FolderAccessPolicy(_currentUser, new FileAccessPolicy(_currentUser)),
            _eventBus);
    }
}
