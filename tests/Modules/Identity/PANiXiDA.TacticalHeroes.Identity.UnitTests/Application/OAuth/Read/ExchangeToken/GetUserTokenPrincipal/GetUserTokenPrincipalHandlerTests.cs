using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Read.ExchangeToken.GetUserTokenPrincipal;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.OAuth.Read.ExchangeToken.GetUserTokenPrincipal;

public sealed class GetUserTokenPrincipalHandlerTests
{
    [Fact(
        DisplayName = "User token principal handler should query the OAuth users repository when repository succeeds")]
    public async Task HandleAsync_Should_ReturnPrincipal_When_RepositorySucceeds()
    {
        var userId = Guid.CreateVersion7();
        var readModel = new OAuthUserTokenPrincipalReadModel([]);
        var repository = Substitute.For<IOAuthUsersRepository>();
        repository.GetTokenPrincipalByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(readModel));
        var handler = new GetUserTokenPrincipalHandler(repository);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            new GetUserTokenPrincipalQuery(userId),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(readModel);
        await repository.Received(1).GetTokenPrincipalByUserIdAsync(userId, cancellationToken);
    }
}
