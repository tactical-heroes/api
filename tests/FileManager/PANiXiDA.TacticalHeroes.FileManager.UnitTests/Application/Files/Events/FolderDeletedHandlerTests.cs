using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Events;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Events;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Files.Events;

public sealed class FolderDeletedHandlerTests
{
    private readonly IFilesRepository _files = Substitute.For<IFilesRepository>();

    private readonly Folder _folder = Folder.Create(
        FolderName.Create("Avatars").Value, FileType.Avatar, userId: null).Value;

    public FolderDeletedHandlerTests()
    {
        _files.GetByFolderForDeletionAsync(Arg.Any<FolderId>(), Arg.Any<CancellationToken>())
            .Returns(new List<File>());
    }

    [Fact(DisplayName = "File deletion subscriber should do nothing when no active files remain")]
    public async Task HandleAsync_Should_DoNothing_When_NoActiveFilesRemain()
    {
        var handler = new FolderDeletedHandler(_files);

        await handler.HandleAsync(new FolderDeleted(_folder.Id.Value), TestContext.Current.CancellationToken);

        _files.DidNotReceiveWithAnyArgs().Update(null!);
        _files.ReceivedCalls().ShouldHaveSingleItem();
    }

    [Theory(DisplayName = "File deletion subscriber should begin deletion for all files when active files are present")]
    [InlineData(3)]
    [InlineData(107)]
    public async Task HandleAsync_Should_BeginDeletionForAllFiles_When_ActiveFilesArePresent(int count)
    {
        var files = Enumerable.Range(0, count).Select(_ => CreateFile()).ToList();
        files[1].CompleteUpload(FileContentType.Create("text/plain").Value, FileSize.Create(1).Value);
        files[2].BeginDeletion();
        _files.GetByFolderForDeletionAsync(_folder.Id, Arg.Any<CancellationToken>()).Returns(files);
        var handler = new FolderDeletedHandler(_files);

        await handler.HandleAsync(new FolderDeleted(_folder.Id.Value), TestContext.Current.CancellationToken);

        foreach (var file in files)
        {
            file.Status.ShouldBe(FileStatus.Deleting);
            _files.Received(1).Update(file);
        }
        await _files.Received(1).GetByFolderForDeletionAsync(_folder.Id, TestContext.Current.CancellationToken);
    }

    private File CreateFile()
    {
        var file = File.Create(FileName.Create("avatar.png").Value, FileType.Avatar, userId: null).Value;
        file.MoveTo(_folder);
        return file;
    }
}
