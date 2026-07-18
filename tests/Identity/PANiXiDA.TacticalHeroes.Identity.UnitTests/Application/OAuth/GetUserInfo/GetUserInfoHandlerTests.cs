using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.GetUserInfo;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.OAuth.GetUserInfo;

public sealed class GetUserInfoHandlerTests
{
    [Fact(DisplayName = "User info handler should query the OAuth users repository")]
    public async Task HandleAsync_Should_ReturnUserInfo_When_RepositorySucceeds()
    {
        var userId = Guid.CreateVersion7();
        var readModel = new UserInfoReadModel(
            userId,
            "hero",
            "hero@example.com",
            true,
            ["admin"]);
        var repository = Substitute.For<IOAuthUsersRepository>();
        repository.GetUserInfoByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(readModel));
        var handler = new GetUserInfoHandler(repository);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            new GetUserInfoQuery(userId),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(readModel);
        await repository.Received(1).GetUserInfoByUserIdAsync(userId, cancellationToken);
    }
}
