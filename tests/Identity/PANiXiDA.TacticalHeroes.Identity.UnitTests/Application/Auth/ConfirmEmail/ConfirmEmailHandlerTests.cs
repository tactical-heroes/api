using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Auth.ConfirmEmail;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Auth.ConfirmEmail;

public sealed class ConfirmEmailHandlerTests
{
    [Fact(DisplayName = "Confirm email handler should delegate confirmation to credentials service")]
    public async Task HandleAsync_Should_ReturnSuccess_When_CredentialsServiceSucceeds()
    {
        var userId = Guid.CreateVersion7();
        var service = Substitute.For<IUserCredentialsService>();
        service.ConfirmEmailAsync(
                userId,
                "confirmation-token",
                Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var handler = new ConfirmEmailHandler(service);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            new ConfirmEmailCommand(userId, "confirmation-token"),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await service.Received(1).ConfirmEmailAsync(
            userId,
            "confirmation-token",
            cancellationToken);
    }
}
