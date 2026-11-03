using NSubstitute.ExceptionExtensions;

using PANiXiDA.Core.Application.Messaging.EventBus;

using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.ReconcileDeletions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Events;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Folders.ReconcileDeletions;

public sealed class ReconcileFolderDeletionsHandlerTests
{
    private readonly IFoldersRepository _folders = Substitute.For<IFoldersRepository>();
    private readonly IEventBus _events = Substitute.For<IEventBus>();

    [Fact(DisplayName = "Folder reconciliation should do nothing when no deleted folders have active contents")]
    public async Task HandleAsync_Should_DoNothing_When_NoDeletedFoldersHaveActiveContents()
    {
        _folders.GetDeletedWithActiveContentsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Folder>());
        var handler = new ReconcileFolderDeletionsHandler(_folders, _events);

        var result = await handler.HandleAsync(
            new ReconcileFolderDeletionsCommand(), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        _events.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Folder reconciliation should publish deletion when deleted folders have active contents")]
    public async Task HandleAsync_Should_PublishDeletion_When_DeletedFoldersHaveActiveContents()
    {
        var folders = new List<Folder> { CreateFolder("First"), CreateFolder("Second") };
        _folders.GetDeletedWithActiveContentsAsync(Arg.Any<CancellationToken>())
            .Returns(folders);
        var handler = new ReconcileFolderDeletionsHandler(_folders, _events);

        var result = await handler.HandleAsync(
            new ReconcileFolderDeletionsCommand(), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        foreach (var folder in folders)
        {
            await _events.Received(1).PublishAsync(
                Arg.Is<FolderDeleted>(message => message.FolderId == folder.Id.Value),
                TestContext.Current.CancellationToken);
        }
        _events.ReceivedCalls().Count().ShouldBe(folders.Count);
        _folders.DidNotReceiveWithAnyArgs().Update(null!);
        _folders.DidNotReceiveWithAnyArgs().Delete(null!);
    }

    [Fact(DisplayName = "Folder reconciliation should propagate failure when deletion publication fails")]
    public async Task HandleAsync_Should_PropagateFailure_When_DeletionPublicationFails()
    {
        _folders.GetDeletedWithActiveContentsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Folder> { CreateFolder("Deleted") });
        _events.PublishAsync(Arg.Any<FolderDeleted>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new IOException("Outbox unavailable"));
        var handler = new ReconcileFolderDeletionsHandler(_folders, _events);

        var exception = await Record.ExceptionAsync(() => handler.HandleAsync(
            new ReconcileFolderDeletionsCommand(), TestContext.Current.CancellationToken));

        exception.ShouldBeOfType<IOException>();
    }

    private static Folder CreateFolder(string name)
    {
        return Folder.Create(FolderName.Create(name).Value, FileType.Avatar, userId: null).Value;
    }
}
