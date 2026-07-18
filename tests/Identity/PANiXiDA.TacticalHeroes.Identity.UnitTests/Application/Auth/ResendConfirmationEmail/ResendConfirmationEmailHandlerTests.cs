using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Auth.ResendConfirmationEmail;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Auth.ResendConfirmationEmail;

public sealed class ResendConfirmationEmailHandlerTests
{
    [Fact(DisplayName = "Resend confirmation handler should delegate email request to credentials service")]
    public async Task HandleAsync_Should_ReturnSuccess_When_CredentialsServiceSucceeds()
    {
        var service = Substitute.For<IUserCredentialsService>();
        service.ResendConfirmationEmailAsync(
                "hero@example.com",
                Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var handler = new ResendConfirmationEmailHandler(service);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            new ResendConfirmationEmailCommand("hero@example.com"),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await service.Received(1).ResendConfirmationEmailAsync(
            "hero@example.com",
            cancellationToken);
    }
}
