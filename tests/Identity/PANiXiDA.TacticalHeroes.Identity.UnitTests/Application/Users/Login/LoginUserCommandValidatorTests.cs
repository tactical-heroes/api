using PANiXiDA.TacticalHeroes.Identity.Application.Users.Login;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Users.Login;

public sealed class LoginUserCommandValidatorTests
{
    [Fact(DisplayName = "Login validator should accept valid input when command is valid")]
    public void Validate_Should_ReturnValidResult_When_CommandIsValid()
    {
        var validator = new LoginUserCommandValidator();

        var result = validator.Validate(new LoginUserCommand("hero@example.com", "StrongPassword1!"));

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "Login validator should reject invalid input when command is invalid")]
    public void Validate_Should_ReturnErrors_When_CommandIsInvalid()
    {
        var validator = new LoginUserCommandValidator();

        var result = validator.Validate(new LoginUserCommand("invalid", ""));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(LoginUserCommand.Email));
        result.Errors.ShouldContain(error => error.PropertyName == nameof(LoginUserCommand.Password));
    }
}
