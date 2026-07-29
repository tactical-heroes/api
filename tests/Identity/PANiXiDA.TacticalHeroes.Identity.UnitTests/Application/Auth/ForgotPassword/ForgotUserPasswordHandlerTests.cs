using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Auth.ForgotPassword;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Auth.ForgotPassword;

public sealed class ForgotUserPasswordHandlerTests
{
    [Fact(DisplayName = "Forgot password handler should delegate reset request to credentials service when credentials service succeeds")]
    public async Task HandleAsync_Should_ReturnSuccess_When_CredentialsServiceSucceeds()
    {
        var service = Substitute.For<IUserCredentialsService>();
        service.ForgotPasswordAsync(
                "hero@example.com",
                Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var handler = new ForgotUserPasswordHandler(service);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            new ForgotUserPasswordCommand("hero@example.com"),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await service.Received(1).ForgotPasswordAsync(
            "hero@example.com",
            cancellationToken);
    }
}
