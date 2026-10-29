using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetDetails;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Files.GetDetails;

public sealed class GetFileDetailsHandlerTests
{
    [Theory(DisplayName = "File details handler should apply the read policy when type owner and permissions vary")]
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
        var file = new FileDetailsReadModel(
            Guid.NewGuid(),
            "file.txt",
            type,
            type == "Personal" ? own ? userId : Guid.NewGuid() : null,
            null,
            "Ready",
            "text/plain",
            1024);
        var repository = Substitute.For<IFilesReadRepository>();
        var cancellationToken = TestContext.Current.CancellationToken;
        repository.GetDetailsByIdAsync(file.Id, cancellationToken).Returns(file);
        var handler = new GetFileDetailsHandler(repository, currentUser, new FileAccessPolicy(currentUser));

        var result = await handler.HandleAsync(new GetFileDetailsQuery(file.Id), cancellationToken);

        if (allowed)
        {
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldBeSameAs(file);
        }
        else
        {
            result.ShouldHaveSingleError(ErrorType.NotFound, "File was not found.");
        }
        await repository.Received(1).GetDetailsByIdAsync(file.Id, cancellationToken);
    }

    [Fact(DisplayName = "File details handler should return not found when file is missing")]
    public async Task HandleAsync_Should_ReturnNotFound_When_FileIsMissing()
    {
        var currentUser = CreateCurrentUser(Guid.NewGuid());
        var repository = Substitute.For<IFilesReadRepository>();
        var handler = new GetFileDetailsHandler(repository, currentUser, new FileAccessPolicy(currentUser));

        var result = await handler.HandleAsync(
            new GetFileDetailsQuery(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(ErrorType.NotFound, "File was not found.");
    }

    [Theory(DisplayName = "File details handler should reject caller when authentication or user id is unavailable")]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task HandleAsync_Should_RejectCaller_When_AuthenticationOrUserIdIsUnavailable(
        bool authenticated,
        bool hasUserId)
    {
        var currentUser = CreateCurrentUser(hasUserId ? Guid.NewGuid() : null);
        currentUser.IsAuthenticated.Returns(authenticated);
        var repository = Substitute.For<IFilesReadRepository>();
        var handler = new GetFileDetailsHandler(repository, currentUser, new FileAccessPolicy(currentUser));

        var result = await handler.HandleAsync(
            new GetFileDetailsQuery(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(ErrorType.Unauthorized, "A user account is required.");
        repository.ReceivedCalls().ShouldBeEmpty();
    }

    private static ICurrentUser CreateCurrentUser(Guid? userId)
    {
        var user = Substitute.For<ICurrentUser>();
        user.UserId.Returns(userId);
        user.IsAuthenticated.Returns(true);
        return user;
    }
}
