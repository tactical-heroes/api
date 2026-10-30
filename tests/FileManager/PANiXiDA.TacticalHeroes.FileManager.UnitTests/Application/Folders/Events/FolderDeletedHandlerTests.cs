using NSubstitute.ExceptionExtensions;

using PANiXiDA.Core.Application.Messaging.EventBus;

using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Events;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Events;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Folders.Events;

public sealed class FolderDeletedHandlerTests
{
    private readonly IFoldersRepository _folders = Substitute.For<IFoldersRepository>();
    private readonly IEventBus _eventBus = Substitute.For<IEventBus>();

    private readonly Folder _parent = Folder.Create(
        FolderName.Create("Parent").Value, FileType.Personal, UserId.Create(Guid.NewGuid()).Value).Value;

    public FolderDeletedHandlerTests()
    {
        _folders.GetChildrenBatchAsync(Arg.Any<FolderId>(), Arg.Any<CancellationToken>())
            .Returns(new List<Folder>());
    }

    [Fact(DisplayName = "Folder deletion event should do nothing when no active children remain")]
    public async Task HandleAsync_Should_DoNothing_When_NoActiveChildrenRemain()
    {
        var handler = CreateHandler();

        await handler.HandleAsync(new FolderDeleted(_parent.Id.Value), TestContext.Current.CancellationToken);

        _folders.DidNotReceiveWithAnyArgs().Delete(null!);
        _eventBus.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Folder deletion event should cascade locally when direct children are present")]
    public async Task HandleAsync_Should_CascadeLocally_When_DirectChildrenArePresent()
    {
        var children = Enumerable.Range(0, 2)
            .Select(index => _parent.CreateChild(FolderName.Create($"Child {index}").Value))
            .ToList();
        _folders.GetChildrenBatchAsync(_parent.Id, Arg.Any<CancellationToken>()).Returns(children);

        await CreateHandler().HandleAsync(new FolderDeleted(_parent.Id.Value), TestContext.Current.CancellationToken);

        foreach (var child in children)
        {
            _folders.Received(1).Delete(child);
            await _eventBus.Received(1).PublishAsync(
                Arg.Is<FolderDeleted>(e => e.FolderId == child.Id.Value), TestContext.Current.CancellationToken);
        }
        await _eventBus.Received(1).PublishAsync(
            Arg.Is<FolderDeleted>(e => e.FolderId == _parent.Id.Value), TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Folder deletion event should enqueue continuation when a batch is full")]
    public async Task HandleAsync_Should_EnqueueContinuation_When_ABatchIsFull()
    {
        var children = Enumerable.Range(0, 100)
            .Select(index => _parent.CreateChild(FolderName.Create($"Child {index}").Value))
            .ToList();
        _folders.GetChildrenBatchAsync(_parent.Id, Arg.Any<CancellationToken>()).Returns(children);

        await CreateHandler().HandleAsync(new FolderDeleted(_parent.Id.Value), TestContext.Current.CancellationToken);

        await _eventBus.Received(1).PublishAsync(
            Arg.Is<FolderDeleted>(e => e.FolderId == _parent.Id.Value), TestContext.Current.CancellationToken);
        await _folders.Received(1).GetChildrenBatchAsync(_parent.Id, TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Folder deletion event should propagate failure when child event publication fails")]
    public async Task HandleAsync_Should_PropagateFailure_When_ChildEventPublicationFails()
    {
        _folders.GetChildrenBatchAsync(_parent.Id, Arg.Any<CancellationToken>())
            .Returns([_parent.CreateChild(FolderName.Create("Child").Value)]);
        _eventBus.PublishAsync(Arg.Any<FolderDeleted>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new IOException("Outbox unavailable."));

        var exception = await Record.ExceptionAsync(() => CreateHandler().HandleAsync(
            new FolderDeleted(_parent.Id.Value), TestContext.Current.CancellationToken));

        exception.ShouldBeOfType<IOException>();
    }

    private FolderDeletedHandler CreateHandler()
    {
        return new FolderDeletedHandler(_folders, _eventBus);
    }
}
