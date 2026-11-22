using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Move;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Services;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Folders.Move;

public sealed class MoveFolderHandlerTests
{
    private readonly IFoldersRepository _folders = Substitute.For<IFoldersRepository>();
    private readonly IFoldersReadRepository _foldersReadRepository = Substitute.For<IFoldersReadRepository>();
    private readonly IFolderHierarchyLock _hierarchyLock = Substitute.For<IFolderHierarchyLock>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly Guid _userId = Guid.NewGuid();

    public MoveFolderHandlerTests()
    {
        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.UserId.Returns(_userId);
        _currentUser.HasPermission("file-manager.personal-files.update.own").Returns(true);
    }

    [Fact(DisplayName = "Move folder handler should update only the source when parent is compatible")]
    public async Task HandleAsync_Should_UpdateOnlyTheSource_When_ParentIsCompatible()
    {
        var folder = CreateFolder(_userId);
        var parent = CreateFolder(_userId);
        _folders.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);
        _folders.GetByIdAsync(parent.Id, Arg.Any<CancellationToken>()).Returns(parent);

        var result = await CreateHandler().HandleAsync(
            new MoveFolderCommand(folder.Id.Value, parent.Id.Value), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        folder.ParentId.ShouldBe(parent.Id);
        await _hierarchyLock.Received(2).AcquireAsync(TestContext.Current.CancellationToken);
        _folders.Received(1).Update(Arg.Any<Folder>());
        _folders.Received(1).Update(folder);
        await _foldersReadRepository.Received(1).HasAncestorAsync(
            parent.Id.Value, folder.Id.Value, TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Move folder handler should wait before reading when lock acquisition is pending")]
    public async Task HandleAsync_Should_WaitBeforeReading_When_LockAcquisitionIsPending()
    {
        var folder = CreateFolder(_userId);
        var acquisition = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _hierarchyLock.AcquireAsync(Arg.Any<CancellationToken>()).Returns(acquisition.Task);
        _folders.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);
        var handler = CreateHandler();

        var pending = handler.HandleAsync(
            new MoveFolderCommand(folder.Id.Value), TestContext.Current.CancellationToken);
        var queriedBeforeAcquisition = _folders.ReceivedCalls().Any();
        acquisition.SetResult();
        var result = await pending;

        queriedBeforeAcquisition.ShouldBeFalse();
        result.IsSuccess.ShouldBeTrue();
        _folders.Received(1).Update(folder);
    }

    [Fact(DisplayName = "Move folder handler should detach the folder when parent is omitted")]
    public async Task HandleAsync_Should_DetachFolder_When_ParentIsOmitted()
    {
        var root = CreateFolder(_userId);
        var folder = root.CreateChild(FolderName.Create("Child").Value);
        _folders.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);

        var result = await CreateHandler().HandleAsync(
            new MoveFolderCommand(folder.Id.Value), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        folder.ParentId.ShouldBeNull();
        _folders.Received(1).Update(folder);
        _foldersReadRepository.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Move folder handler should preserve placement when target descends from the source")]
    public async Task HandleAsync_Should_PreservePlacement_When_TargetDescendsFromTheSource()
    {
        var folder = CreateFolder(_userId);
        var parent = folder.CreateChild(FolderName.Create("Child").Value);
        _folders.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);
        _folders.GetByIdAsync(parent.Id, Arg.Any<CancellationToken>()).Returns(parent);
        _foldersReadRepository.HasAncestorAsync(parent.Id.Value, folder.Id.Value, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await CreateHandler().HandleAsync(
            new MoveFolderCommand(folder.Id.Value, parent.Id.Value), TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(
            ErrorType.Validation, "A folder cannot be moved into one of its descendants.")
            .ShouldHaveField(nameof(Folder.ParentId));
        folder.ParentId.ShouldBeNull();
        _folders.DidNotReceiveWithAnyArgs().Update(null!);
    }

    [Fact(DisplayName = "Move folder handler should reject the parent when target is the source")]
    public async Task HandleAsync_Should_RejectParent_When_TargetIsTheSource()
    {
        var folder = CreateFolder(_userId);
        _folders.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);

        var result = await CreateHandler().HandleAsync(
            new MoveFolderCommand(folder.Id.Value, folder.Id.Value), TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(ErrorType.Validation, "A folder cannot be its own parent.");
        folder.ParentId.ShouldBeNull();
        _folders.DidNotReceiveWithAnyArgs().Update(null!);
    }

    [Theory(DisplayName = "Move folder handler should conceal the source when source is unavailable")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_ConcealSource_When_SourceIsUnavailable(bool forbidden)
    {
        var folder = CreateFolder(Guid.NewGuid());
        if (forbidden)
        {
            _folders.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);
        }

        var result = await CreateHandler().HandleAsync(
            new MoveFolderCommand(folder.Id.Value, Guid.NewGuid()), TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(ErrorType.NotFound, "Folder was not found.");
        _folders.DidNotReceiveWithAnyArgs().Update(null!);
        _foldersReadRepository.ReceivedCalls().ShouldBeEmpty();
    }

    [Theory(DisplayName = "Move folder handler should conceal the parent when parent is unavailable")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_ConcealParent_When_ParentIsUnavailable(bool forbidden)
    {
        var folder = CreateFolder(_userId);
        var parent = CreateFolder(Guid.NewGuid());
        _folders.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);
        if (forbidden)
        {
            _folders.GetByIdAsync(parent.Id, Arg.Any<CancellationToken>()).Returns(parent);
        }

        var result = await CreateHandler().HandleAsync(
            new MoveFolderCommand(folder.Id.Value, parent.Id.Value), TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(ErrorType.NotFound, "Folder was not found.");
        folder.ParentId.ShouldBeNull();
        _folders.DidNotReceiveWithAnyArgs().Update(null!);
        _foldersReadRepository.ReceivedCalls().ShouldBeEmpty();
    }

    [Theory(DisplayName = "Move folder handler should avoid persistence when identifier is invalid")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_AvoidPersistence_When_IdentifierIsInvalid(bool invalidParent)
    {
        var command = new MoveFolderCommand(invalidParent ? Guid.NewGuid() : Guid.Empty, Guid.Empty);

        var result = await CreateHandler().HandleAsync(command, TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        _hierarchyLock.ReceivedCalls().ShouldBeEmpty();
        _folders.ReceivedCalls().ShouldBeEmpty();
        _foldersReadRepository.ReceivedCalls().ShouldBeEmpty();
    }

    private static Folder CreateFolder(Guid userId)
    {
        return Folder.Create(
            FolderName.Create("Documents").Value, FileType.Personal, UserId.Create(userId).Value).Value;
    }

    private MoveFolderHandler CreateHandler()
    {
        return new MoveFolderHandler(
            _folders,
            _foldersReadRepository,
            new FolderAccessService(
                _folders, _hierarchyLock, new FolderAccessPolicy(_currentUser, new FileAccessPolicy(_currentUser))));
    }
}
