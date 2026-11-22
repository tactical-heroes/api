using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Services;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Folders.Services;

public sealed class FolderAccessServiceTests
{
    private readonly IFoldersRepository _folders = Substitute.For<IFoldersRepository>();
    private readonly IFolderHierarchyLock _hierarchyLock = Substitute.For<IFolderHierarchyLock>();
    private readonly ICurrentUser _user = Substitute.For<ICurrentUser>();
    private readonly Guid _userId = Guid.NewGuid();

    public FolderAccessServiceTests()
    {
        _user.IsAuthenticated.Returns(true);
        _user.UserId.Returns(_userId);
    }

    [Theory(DisplayName = "Folder access service should enforce the requested permission when permissions vary")]
    [InlineData(true, "file-manager.personal-files.create.own", false, true)]
    [InlineData(true, "file-manager.personal-files.update.own", false, false)]
    [InlineData(true, "", false, false)]
    [InlineData(true, "file-manager.personal-files.create.own", true, false)]
    [InlineData(true, "file-manager.personal-files.create.all", true, true)]
    [InlineData(false, "file-manager.personal-files.update.own", false, true)]
    [InlineData(false, "file-manager.personal-files.create.own", false, false)]
    [InlineData(false, "", false, false)]
    [InlineData(false, "file-manager.personal-files.update.own", true, false)]
    [InlineData(false, "file-manager.personal-files.update.all", true, true)]
    public async Task GetAsync_Should_EnforceRequestedPermission_When_PermissionsVary(
        bool creating, string permission, bool foreignOwner, bool allowed)
    {
        _user.HasPermission(permission).Returns(true);
        var folder = Folder.Create(
            FolderName.Create("Documents").Value,
            FileType.Personal,
            UserId.Create(foreignOwner ? Guid.NewGuid() : _userId).Value).Value;
        _folders.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);
        var service = CreateService();

        var result = await (creating
            ? service.GetForCreateAsync(folder.Id, TestContext.Current.CancellationToken)
            : service.GetForUpdateAsync(folder.Id, TestContext.Current.CancellationToken));

        result.IsSuccess.ShouldBe(allowed);
        if (allowed)
        {
            result.Value.ShouldBeSameAs(folder);
        }
        else
        {
            result.ShouldHaveSingleError(ErrorType.NotFound, "Folder was not found.");
        }
        await _hierarchyLock.Received(1).AcquireAsync(TestContext.Current.CancellationToken);
        _folders.DidNotReceiveWithAnyArgs().Update(default!);
    }

    [Theory(DisplayName = "Folder access service should protect lookup when lock acquisition is pending")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetAsync_Should_ProtectLookup_When_LockAcquisitionIsPending(bool creating)
    {
        var folderId = FolderId.New();
        var acquisition = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _hierarchyLock.AcquireAsync(Arg.Any<CancellationToken>()).Returns(acquisition.Task);
        var service = CreateService();

        var pending = creating
            ? service.GetForCreateAsync(folderId, TestContext.Current.CancellationToken)
            : service.GetForUpdateAsync(folderId, TestContext.Current.CancellationToken);
        var readBeforeAcquisition = _folders.ReceivedCalls().Any();
        acquisition.SetResult();
        var result = await pending;

        readBeforeAcquisition.ShouldBeFalse();
        result.ShouldHaveSingleError(ErrorType.NotFound, "Folder was not found.");
        await _folders.Received(1).GetByIdAsync(folderId, TestContext.Current.CancellationToken);
    }

    [Theory(DisplayName = "Folder access service should skip lookup when lock acquisition fails")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetAsync_Should_SkipLookup_When_LockAcquisitionFails(bool creating)
    {
        var exception = new InvalidOperationException("Transaction is required.");
        _hierarchyLock.AcquireAsync(Arg.Any<CancellationToken>()).Returns(Task.FromException(exception));
        var service = CreateService();

        var thrown = await Should.ThrowAsync<InvalidOperationException>(() => creating
            ? service.GetForCreateAsync(FolderId.New(), TestContext.Current.CancellationToken)
            : service.GetForUpdateAsync(FolderId.New(), TestContext.Current.CancellationToken));

        thrown.ShouldBeSameAs(exception);
        _folders.ReceivedCalls().ShouldBeEmpty();
    }

    private FolderAccessService CreateService()
    {
        return new FolderAccessService(
            _folders, _hierarchyLock, new FolderAccessPolicy(_user, new FileAccessPolicy(_user)));
    }
}
