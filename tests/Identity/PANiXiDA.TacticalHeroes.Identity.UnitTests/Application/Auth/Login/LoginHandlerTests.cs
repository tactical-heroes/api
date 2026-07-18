using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Login;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Auth.Login;

public sealed class LoginHandlerTests
{
    [Fact(DisplayName = "Login handler should delegate authentication to credentials service")]
    public async Task HandleAsync_Should_ReturnAuthenticatedUser_When_CredentialsServiceSucceeds()
    {
        var authenticatedUser = new AuthenticatedUserReadModel(
            Guid.CreateVersion7(),
            "hero@example.com",
            "hero",
            []);
        var service = Substitute.For<IUserCredentialsService>();
        service.LoginAsync(
                "hero@example.com",
                "StrongPassword1!",
                Arg.Any<CancellationToken>())
            .Returns(Result.Success(authenticatedUser));
        var handler = new LoginHandler(service);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            new LoginCommand("hero@example.com", "StrongPassword1!"),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(authenticatedUser);
        await service.Received(1).LoginAsync(
            "hero@example.com",
            "StrongPassword1!",
            cancellationToken);
    }
}
