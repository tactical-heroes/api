using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.GetClientTokenPrincipal;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.OAuth.GetClientTokenPrincipal;

public sealed class GetClientTokenPrincipalHandlerTests
{
    [Fact(DisplayName = "Client token principal handler should query the OAuth clients repository")]
    public async Task HandleAsync_Should_ReturnPrincipal_When_RepositorySucceeds()
    {
        var readModel = new OAuthClientTokenPrincipalReadModel([]);
        var repository = Substitute.For<IOAuthClientsRepository>();
        repository.GetTokenPrincipalByClientIdAsync(
                "tactical-heroes-service",
                Arg.Any<CancellationToken>())
            .Returns(Result.Success(readModel));
        var handler = new GetClientTokenPrincipalHandler(repository);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            new GetClientTokenPrincipalQuery("tactical-heroes-service"),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(readModel);
        await repository.Received(1).GetTokenPrincipalByClientIdAsync(
            "tactical-heroes-service",
            cancellationToken);
    }
}
