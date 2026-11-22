using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.GetDetails;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Policies;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Folders.GetDetails;

public sealed class GetFolderDetailsHandlerTests
{
    [Theory(DisplayName = "Folder details handler should apply the read policy when type owner and permissions vary")]
    [InlineData("Personal", true, "file-manager.personal-files.read.own", true)]
    [InlineData("Personal", false, "file-manager.personal-files.read.own", false)]
    [InlineData("Personal", false, "file-manager.personal-files.read.all", true)]
    [InlineData("Personal", true, "file-manager.avatars.read", false)]
    [InlineData("Personal", true, "", false)]
    [InlineData("Avatar", false, "file-manager.avatars.read", true)]
    [InlineData("Avatar", false, "file-manager.personal-files.read.all", false)]
    public async Task HandleAsync_Should_ApplyReadPolicy_When_TypeOwnerAndPermissionsVary(
        string type, bool own, string permission, bool allowed)
    {
        var userId = Guid.NewGuid();
        var currentUser = CreateCurrentUser(userId);
        currentUser.HasPermission(permission).Returns(true);
        var folder = new FolderDetailsReadModel(
            Guid.NewGuid(),
            "Documents",
            type,
            type == "Personal" ? own ? userId : Guid.NewGuid() : null,
            Guid.NewGuid(),
            "Parent");
        var repository = Substitute.For<IFoldersReadRepository>();
        var cancellationToken = TestContext.Current.CancellationToken;
        repository.GetDetailsByIdAsync(folder.Id, cancellationToken).Returns(folder);
        var handler = new GetFolderDetailsHandler(
            repository,
            new FolderAccessPolicy(currentUser, new FileAccessPolicy(currentUser)));

        var result = await handler.HandleAsync(new GetFolderDetailsQuery(folder.Id), cancellationToken);

        if (allowed)
        {
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldBeSameAs(folder);
        }
        else
        {
            result.ShouldHaveSingleError(ErrorType.NotFound, "Folder was not found.");
        }
        await repository.Received(1).GetDetailsByIdAsync(folder.Id, cancellationToken);
    }

    [Fact(DisplayName = "Folder details handler should return not found when folder is missing")]
    public async Task HandleAsync_Should_ReturnNotFound_When_FolderIsMissing()
    {
        var currentUser = CreateCurrentUser(Guid.NewGuid());
        var repository = Substitute.For<IFoldersReadRepository>();
        var handler = new GetFolderDetailsHandler(
            repository,
            new FolderAccessPolicy(currentUser, new FileAccessPolicy(currentUser)));

        var result = await handler.HandleAsync(
            new GetFolderDetailsQuery(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(ErrorType.NotFound, "Folder was not found.");
    }

    private static ICurrentUser CreateCurrentUser(Guid? userId)
    {
        var user = Substitute.For<ICurrentUser>();
        user.UserId.Returns(userId);
        user.IsAuthenticated.Returns(true);
        return user;
    }
}
