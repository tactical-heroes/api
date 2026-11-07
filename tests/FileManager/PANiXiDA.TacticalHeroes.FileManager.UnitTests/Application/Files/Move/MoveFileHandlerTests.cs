using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Move;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Services;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Files.Move;

public sealed class MoveFileHandlerTests
{
    private readonly IFilesRepository _files = Substitute.For<IFilesRepository>();
    private readonly IFoldersRepository _folders = Substitute.For<IFoldersRepository>();
    private readonly IFolderHierarchyLock _hierarchyLock = Substitute.For<IFolderHierarchyLock>();
    private readonly ICurrentUser _user = Substitute.For<ICurrentUser>();
    private readonly Guid _userId = Guid.NewGuid();

    public MoveFileHandlerTests()
    {
        _user.IsAuthenticated.Returns(true);
        _user.UserId.Returns(_userId);
    }

    [Theory(DisplayName = "Move file handler should update only the file when destination is compatible")]
    [InlineData("Personal", "file-manager.personal-files.update.own", false)]
    [InlineData("Personal", "file-manager.personal-files.update.all", true)]
    [InlineData("Avatar", "file-manager.avatars.update", false)]
    public async Task HandleAsync_Should_UpdateOnlyTheFile_When_DestinationIsCompatible(
        string type, string permission, bool foreignOwner)
    {
        _user.HasPermission(permission).Returns(true);
        var ownerId = foreignOwner ? Guid.NewGuid() : _userId;
        var folder = CreateFolder(type, ownerId);
        var file = CreateFile(type, ownerId);
        file.CompleteUpload(FileContentType.Create("text/plain").Value, FileSize.Create(4).Value);
        _files.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        _folders.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);
        var key = file.StorageKey;

        var result = await CreateHandler().HandleAsync(
            new MoveFileCommand(file.Id.Value, folder.Id.Value), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        file.FolderId.ShouldBe(folder.Id);
        file.StorageKey.ShouldBeSameAs(key);
        file.Name.Value.ShouldBe("file.txt");
        file.ContentType!.Value.ShouldBe("text/plain");
        file.Size!.Value.ShouldBe(4);
        file.Status.Name.ShouldBe("Ready");
        _files.Received(1).Update(file);
        _folders.DidNotReceiveWithAnyArgs().Update(default!);
        await _hierarchyLock.Received(1).AcquireAsync(TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Move file handler should protect folder lookup when lock acquisition is pending")]
    public async Task HandleAsync_Should_ProtectFolderLookup_When_LockAcquisitionIsPending()
    {
        _user.HasPermission("file-manager.personal-files.update.own").Returns(true);
        var file = CreateFile("Personal", _userId);
        var folder = CreateFolder("Personal", _userId);
        var acquisition = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _hierarchyLock.AcquireAsync(Arg.Any<CancellationToken>()).Returns(acquisition.Task);
        _files.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        _folders.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);

        var pending = CreateHandler().HandleAsync(
            new MoveFileCommand(file.Id.Value, folder.Id.Value), TestContext.Current.CancellationToken);
        var fileWasRead = _files.ReceivedCalls().Any();
        var folderWasRead = _folders.ReceivedCalls().Any();
        acquisition.SetResult();
        var result = await pending;

        fileWasRead.ShouldBeTrue();
        folderWasRead.ShouldBeFalse();
        result.IsSuccess.ShouldBeTrue();
        file.FolderId.ShouldBe(folder.Id);
    }

    [Fact(DisplayName = "Move file handler should detach the file when folder is omitted")]
    public async Task HandleAsync_Should_DetachFile_When_FolderIsOmitted()
    {
        _user.HasPermission("file-manager.personal-files.update.own").Returns(true);
        var file = CreateFile("Personal", _userId);
        file.MoveTo(CreateFolder("Personal", _userId));
        _files.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);

        var result = await CreateHandler().HandleAsync(
            new MoveFileCommand(file.Id.Value), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        file.FolderId.ShouldBeNull();
        _files.Received(1).Update(file);
        _folders.ReceivedCalls().ShouldBeEmpty();
        _hierarchyLock.ReceivedCalls().ShouldBeEmpty();
    }

    [Theory(DisplayName = "Move file handler should conceal the source when source is unavailable")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_ConcealSource_When_SourceIsUnavailable(bool denied)
    {
        var file = CreateFile("Personal", _userId);
        if (denied)
        {
            _files.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        }

        var result = await CreateHandler().HandleAsync(
            new MoveFileCommand(file.Id.Value, Guid.NewGuid()), TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(ErrorType.NotFound, "File was not found.");
        _files.DidNotReceiveWithAnyArgs().Update(default!);
        _folders.ReceivedCalls().ShouldBeEmpty();
        _hierarchyLock.ReceivedCalls().ShouldBeEmpty();
    }

    [Theory(DisplayName = "Move file handler should conceal the destination when destination is unavailable")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_ConcealDestination_When_DestinationIsUnavailable(bool denied)
    {
        _user.HasPermission("file-manager.personal-files.update.own").Returns(true);
        var file = CreateFile("Personal", _userId);
        var folder = CreateFolder("Personal", Guid.NewGuid());
        _files.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        if (denied)
        {
            _folders.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);
        }

        var result = await CreateHandler().HandleAsync(
            new MoveFileCommand(file.Id.Value, folder.Id.Value), TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(ErrorType.NotFound, "Folder was not found.");
        file.FolderId.ShouldBeNull();
        _files.DidNotReceiveWithAnyArgs().Update(default!);
    }

    [Theory(DisplayName = "Move file handler should preserve placement when destination is incompatible")]
    [InlineData("Avatar", false)]
    [InlineData("Personal", true)]
    public async Task HandleAsync_Should_PreservePlacement_When_DestinationIsIncompatible(
        string destinationType, bool differentOwner)
    {
        _user.HasPermission("file-manager.personal-files.update.all").Returns(true);
        _user.HasPermission("file-manager.avatars.update").Returns(true);
        var file = CreateFile("Personal", _userId);
        var original = CreateFolder("Personal", _userId);
        file.MoveTo(original);
        var folder = CreateFolder(destinationType, differentOwner ? Guid.NewGuid() : _userId);
        _files.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        _folders.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);

        var result = await CreateHandler().HandleAsync(
            new MoveFileCommand(file.Id.Value, folder.Id.Value), TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldAllBe(error => error.Type == ErrorType.Validation);
        file.FolderId.ShouldBe(original.Id);
        _files.DidNotReceiveWithAnyArgs().Update(default!);
        _folders.DidNotReceiveWithAnyArgs().Update(default!);
    }

    [Theory(DisplayName = "Move file handler should preserve placement when deletion has started")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_PreservePlacement_When_DeletionHasStarted(bool detach)
    {
        _user.HasPermission("file-manager.personal-files.update.own").Returns(true);
        var file = CreateFile("Personal", _userId);
        var folder = CreateFolder("Personal", _userId);
        file.MoveTo(folder);
        file.BeginDeletion();
        _files.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        _folders.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);

        var result = await CreateHandler().HandleAsync(
            new MoveFileCommand(file.Id.Value, detach ? null : folder.Id.Value), TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(ErrorType.Conflict, "A file being deleted cannot be moved.");
        file.FolderId.ShouldBe(folder.Id);
        _files.DidNotReceiveWithAnyArgs().Update(default!);
    }

    [Theory(DisplayName = "Move file handler should avoid persistence when identifier is invalid")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_AvoidPersistence_When_IdentifierIsInvalid(bool invalidDestination)
    {
        var command = new MoveFileCommand(invalidDestination ? Guid.NewGuid() : Guid.Empty, Guid.Empty);

        var result = await CreateHandler().HandleAsync(command, TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        _hierarchyLock.ReceivedCalls().ShouldBeEmpty();
        _files.ReceivedCalls().ShouldBeEmpty();
        _folders.ReceivedCalls().ShouldBeEmpty();
    }

    private static File CreateFile(string type, Guid ownerId)
    {
        return File.Create(
            FileName.Create("file.txt").Value,
            FileType.FromName(type),
            type == "Personal" ? UserId.Create(ownerId).Value : null).Value;
    }

    private static Folder CreateFolder(string type, Guid ownerId)
    {
        return Folder.Create(
            FolderName.Create("Documents").Value,
            FileType.FromName(type),
            type == "Personal" ? UserId.Create(ownerId).Value : null).Value;
    }

    private MoveFileHandler CreateHandler()
    {
        var policy = new FileAccessPolicy(_user);
        return new MoveFileHandler(
            _files, new FolderAccessService(_folders, _hierarchyLock, new FolderAccessPolicy(_user, policy)), policy);
    }
}
