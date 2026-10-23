using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Common;
using PANiXiDA.TacticalHeroes.Identity.Application.Auth.GetAuthenticatedUser;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Auth.GetAuthenticatedUser;

public sealed class GetAuthenticatedUserHandlerTests
{
    [Fact(DisplayName = "Authenticated user handler should return the authentication model when reader succeeds")]
    public async Task HandleAsync_Should_ReturnAuthenticationModel_When_ReaderSucceeds()
    {
        var userId = Guid.CreateVersion7();
        var user = new AuthenticatedUserReadModel(userId, "hero@example.test", "hero", []);
        var reader = Substitute.For<IAuthenticatedUserReader>();
        reader.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(Result.Success(user));
        var handler = new GetAuthenticatedUserHandler(reader);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(new GetAuthenticatedUserQuery(userId), cancellationToken);

        result.Value.ShouldBe(user);
        await reader.Received(1).GetByIdAsync(userId, cancellationToken);
    }
}
