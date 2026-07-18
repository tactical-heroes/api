using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Auth.ResetPassword;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Auth.ResetPassword;

public sealed class ResetPasswordHandlerTests
{
    [Fact(DisplayName = "Reset password handler should delegate password reset to credentials service")]
    public async Task HandleAsync_Should_ReturnSuccess_When_CredentialsServiceSucceeds()
    {
        var userId = Guid.CreateVersion7();
        var service = Substitute.For<IUserCredentialsService>();
        service.ResetPasswordAsync(
                userId,
                "password-reset-token",
                "NewPassword1!",
                Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var handler = new ResetPasswordHandler(service);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            new ResetPasswordCommand(userId, "password-reset-token", "NewPassword1!"),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await service.Received(1).ResetPasswordAsync(
            userId,
            "password-reset-token",
            "NewPassword1!",
            cancellationToken);
    }
}
