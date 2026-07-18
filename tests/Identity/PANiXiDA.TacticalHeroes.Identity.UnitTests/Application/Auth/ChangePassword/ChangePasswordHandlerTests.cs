using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Auth.ChangePassword;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Auth.ChangePassword;

public sealed class ChangePasswordHandlerTests
{
    [Fact(DisplayName = "Change password handler should delegate password change to credentials service")]
    public async Task HandleAsync_Should_ReturnSuccess_When_CredentialsServiceSucceeds()
    {
        var userId = Guid.CreateVersion7();
        var service = Substitute.For<IUserCredentialsService>();
        service.ChangePasswordAsync(
                userId,
                "CurrentPassword1!",
                "NewPassword1!",
                Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var handler = new ChangePasswordHandler(service);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            new ChangePasswordCommand(userId, "CurrentPassword1!", "NewPassword1!"),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await service.Received(1).ChangePasswordAsync(
            userId,
            "CurrentPassword1!",
            "NewPassword1!",
            cancellationToken);
    }
}
