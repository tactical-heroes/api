using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Login;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Auth.Login;

public sealed class LoginHandlerTests
{
    [Fact(
        DisplayName = "Login handler should delegate authentication to credentials service when credentials service succeeds")]
    public async Task HandleAsync_Should_ReturnSecurityProof_When_CredentialsServiceSucceeds()
    {
        var userId = Guid.CreateVersion7();
        var proof = new LoginSecurityProof(userId, "verified-stamp");
        var service = Substitute.For<IUserCredentialsService>();
        service.LoginAsync(
            "hero@example.com",
            "StrongPassword1!",
            Arg.Any<CancellationToken>())
            .Returns(Result.Success(proof));
        var handler = new LoginHandler(service);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            new LoginCommand("hero@example.com", "StrongPassword1!"),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(proof);
        await service.Received(1).LoginAsync(
            "hero@example.com",
            "StrongPassword1!",
            cancellationToken);
    }
}
