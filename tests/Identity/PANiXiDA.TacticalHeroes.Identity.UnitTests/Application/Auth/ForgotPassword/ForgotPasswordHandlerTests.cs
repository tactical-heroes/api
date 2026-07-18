using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Auth.ForgotPassword;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Auth.ForgotPassword;

public sealed class ForgotPasswordHandlerTests
{
    [Fact(DisplayName = "Forgot password handler should delegate reset request to credentials service")]
    public async Task HandleAsync_Should_ReturnSuccess_When_CredentialsServiceSucceeds()
    {
        var service = Substitute.For<IUserCredentialsService>();
        service.ForgotPasswordAsync(
                "hero@example.com",
                Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var handler = new ForgotPasswordHandler(service);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            new ForgotPasswordCommand("hero@example.com"),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await service.Received(1).ForgotPasswordAsync(
            "hero@example.com",
            cancellationToken);
    }
}
