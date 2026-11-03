using NSubstitute.ExceptionExtensions;

using PANiXiDA.Core.Application.Messaging.EventBus;

using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.ReconcileDeletions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Events;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Folders.ReconcileDeletions;

public sealed class ReconcileFolderDeletionsHandlerTests
{
    private readonly IFoldersReadRepository _foldersReadRepository = Substitute.For<IFoldersReadRepository>();
    private readonly IEventBus _events = Substitute.For<IEventBus>();

    [Fact(DisplayName = "Folder reconciliation should do nothing when no deleted folders have active contents")]
    public async Task HandleAsync_Should_DoNothing_When_NoDeletedFoldersHaveActiveContents()
    {
        _foldersReadRepository.GetDeletedIdsWithActiveContentsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Guid>());
        var handler = new ReconcileFolderDeletionsHandler(_foldersReadRepository, _events);

        var result = await handler.HandleAsync(
            new ReconcileFolderDeletionsCommand(), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        _events.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Folder reconciliation should publish deletion when deleted folders have active contents")]
    public async Task HandleAsync_Should_PublishDeletion_When_DeletedFoldersHaveActiveContents()
    {
        var folderIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        _foldersReadRepository.GetDeletedIdsWithActiveContentsAsync(Arg.Any<CancellationToken>())
            .Returns(folderIds);
        var handler = new ReconcileFolderDeletionsHandler(_foldersReadRepository, _events);

        var result = await handler.HandleAsync(
            new ReconcileFolderDeletionsCommand(), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        foreach (var folderId in folderIds)
        {
            await _events.Received(1).PublishAsync(
                Arg.Is<FolderDeleted>(message => message.FolderId == folderId),
                TestContext.Current.CancellationToken);
        }
        _events.ReceivedCalls().Count().ShouldBe(folderIds.Count);
    }

    [Fact(DisplayName = "Folder reconciliation should propagate failure when deletion publication fails")]
    public async Task HandleAsync_Should_PropagateFailure_When_DeletionPublicationFails()
    {
        _foldersReadRepository.GetDeletedIdsWithActiveContentsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Guid> { Guid.NewGuid() });
        _events.PublishAsync(Arg.Any<FolderDeleted>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new IOException("Outbox unavailable"));
        var handler = new ReconcileFolderDeletionsHandler(_foldersReadRepository, _events);

        var exception = await Record.ExceptionAsync(() => handler.HandleAsync(
            new ReconcileFolderDeletionsCommand(), TestContext.Current.CancellationToken));

        exception.ShouldBeOfType<IOException>();
    }
}
