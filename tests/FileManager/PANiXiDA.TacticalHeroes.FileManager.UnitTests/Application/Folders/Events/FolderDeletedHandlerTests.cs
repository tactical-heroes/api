using NSubstitute.ExceptionExtensions;

using PANiXiDA.Core.Application.Messaging.EventBus;

using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Events;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Events;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Folders.Events;

public sealed class FolderDeletedHandlerTests
{
    private readonly IFilesRepository _files = Substitute.For<IFilesRepository>();
    private readonly IFoldersRepository _folders = Substitute.For<IFoldersRepository>();
    private readonly IEventBus _eventBus = Substitute.For<IEventBus>();

    private readonly Folder _parent = Folder.Create(
        FolderName.Create("Parent").Value, FileType.Personal, UserId.Create(Guid.NewGuid()).Value).Value;

    public FolderDeletedHandlerTests()
    {
        _files.GetBatchByFolderForDeletionAsync(Arg.Any<FolderId>(), Arg.Any<CancellationToken>())
            .Returns(new List<File>());
        _folders.GetChildrenBatchAsync(Arg.Any<FolderId>(), Arg.Any<CancellationToken>())
            .Returns(new List<Folder>());
    }

    [Fact(DisplayName = "Folder deletion event should do nothing when no active contents remain")]
    public async Task HandleAsync_Should_DoNothing_When_NoActiveContentsRemain()
    {
        var handler = CreateHandler();

        await handler.HandleAsync(new FolderDeleted(_parent.Id.Value), TestContext.Current.CancellationToken);

        _files.DidNotReceiveWithAnyArgs().Update(null!);
        _folders.DidNotReceiveWithAnyArgs().Delete(null!);
        _eventBus.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Folder deletion event should begin file deletion when files are present")]
    public async Task HandleAsync_Should_BeginFileDeletion_When_FilesArePresent()
    {
        var files = Enumerable.Range(0, 3).Select(_ => CreateFile()).ToList();
        files[1].CompleteUpload(FileContentType.Create("text/plain").Value, FileSize.Create(1).Value);
        files[2].BeginDeletion();
        _files.GetBatchByFolderForDeletionAsync(_parent.Id, Arg.Any<CancellationToken>()).Returns(files);

        await CreateHandler().HandleAsync(new FolderDeleted(_parent.Id.Value), TestContext.Current.CancellationToken);

        foreach (var file in files)
        {
            file.Status.ShouldBe(FileStatus.Deleting);
            _files.Received(1).Update(file);
        }
        await _eventBus.Received(1).PublishAsync(
            Arg.Is<FolderDeleted>(e => e.FolderId == _parent.Id.Value), TestContext.Current.CancellationToken);
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

    [Theory(DisplayName = "Folder deletion event should enqueue continuation when a batch is full")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_EnqueueContinuation_When_ABatchIsFull(bool fullFiles)
    {
        if (fullFiles)
        {
            _files.GetBatchByFolderForDeletionAsync(_parent.Id, Arg.Any<CancellationToken>())
                .Returns(Enumerable.Range(0, 100).Select(_ => CreateFile()).ToList());
        }
        else
        {
            var children = Enumerable.Range(0, 100)
                .Select(index => _parent.CreateChild(FolderName.Create($"Child {index}").Value))
                .ToList();
            _folders.GetChildrenBatchAsync(_parent.Id, Arg.Any<CancellationToken>()).Returns(children);
        }

        await CreateHandler().HandleAsync(new FolderDeleted(_parent.Id.Value), TestContext.Current.CancellationToken);

        await _eventBus.Received(1).PublishAsync(
            Arg.Is<FolderDeleted>(e => e.FolderId == _parent.Id.Value), TestContext.Current.CancellationToken);
        await _files.Received(1).GetBatchByFolderForDeletionAsync(_parent.Id, TestContext.Current.CancellationToken);
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

    private File CreateFile()
    {
        var file = File.Create(FileName.Create("file.txt").Value, FileType.Personal, _parent.UserId).Value;
        file.MoveTo(_parent);
        return file;
    }

    private FolderDeletedHandler CreateHandler()
    {
        return new FolderDeletedHandler(_files, _folders, _eventBus);
    }
}
