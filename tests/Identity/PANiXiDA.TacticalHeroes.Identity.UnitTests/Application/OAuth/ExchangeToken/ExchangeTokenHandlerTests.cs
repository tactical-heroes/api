using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.ExchangeToken;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.OAuth.ExchangeToken;

public sealed class ExchangeTokenHandlerTests
{
    [Fact(DisplayName = "Exchange token handler should query the OAuth users repository")]
    public async Task HandleAsync_Should_ReturnTokenModel_When_RepositorySucceeds()
    {
        var userId = Guid.CreateVersion7();
        var readModel = new ExchangeTokenReadModel([]);
        var repository = Substitute.For<IOAuthUsersRepository>();
        repository.GetExchangeTokenByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(readModel));
        var handler = new ExchangeTokenHandler(repository);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            new ExchangeTokenQuery(userId),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(readModel);
        await repository.Received(1).GetExchangeTokenByUserIdAsync(userId, cancellationToken);
    }
}
