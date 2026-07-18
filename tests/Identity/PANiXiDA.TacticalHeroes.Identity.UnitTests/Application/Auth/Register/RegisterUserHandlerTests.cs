using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Register;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Auth.Register;

public sealed class RegisterUserHandlerTests
{
    [Fact(DisplayName = "Register handler should delegate registration to credentials service")]
    public async Task HandleAsync_Should_ReturnUserId_When_CredentialsServiceSucceeds()
    {
        var userId = Guid.CreateVersion7();
        var service = Substitute.For<IUserCredentialsService>();
        service.RegisterAsync(
                "hero@example.com",
                "hero",
                "StrongPassword1!",
                Arg.Any<CancellationToken>())
            .Returns(Result.Success(userId));
        var handler = new RegisterUserHandler(service);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            new RegisterUserCommand("hero@example.com", "hero", "StrongPassword1!"),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(userId);
        await service.Received(1).RegisterAsync(
            "hero@example.com",
            "hero",
            "StrongPassword1!",
            cancellationToken);
    }
}
