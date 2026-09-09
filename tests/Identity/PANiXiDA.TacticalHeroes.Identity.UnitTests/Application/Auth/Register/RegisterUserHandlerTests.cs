using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Register;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Auth.Register;

public sealed class RegisterUserHandlerTests
{
    [Fact(DisplayName = "Register handler should delegate registration to credentials service when credentials service succeeds")]
    public async Task HandleAsync_Should_ReturnUserId_When_CredentialsServiceSucceeds()
    {
        var userId = Guid.CreateVersion7();
        var service = Substitute.For<IUserCredentialsService>();
        service.RegisterAsync(
            Arg.Is<User>(user => user.Email.Value == "hero@example.com" && !user.ConfirmationStatus.IsConfirmed),
            Arg.Is<UserName>(name => name.Value == "hero"),
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
            Arg.Is<User>(user => user.Email.Value == "hero@example.com" && !user.ConfirmationStatus.IsConfirmed),
            Arg.Is<UserName>(name => name.Value == "hero"),
            "StrongPassword1!",
            cancellationToken);
    }
    [Fact(DisplayName = "Register handler should reject invalid values before persistence when credentials are invalid")]
    public async Task HandleAsync_Should_ReturnValidationFailuresWithoutRegistering_When_CredentialsAreInvalid()
    {
        var service = Substitute.For<IUserCredentialsService>();
        var handler = new RegisterUserHandler(service);

        var result = await handler.HandleAsync(
            new RegisterUserCommand("invalid-email", string.Empty, "StrongPassword1!"),
            TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.Count.ShouldBe(2);
        await service.DidNotReceiveWithAnyArgs().RegisterAsync(
            null!, null!, string.Empty, TestContext.Current.CancellationToken);
    }

}
