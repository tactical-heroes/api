using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Login;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Auth.Login;

public sealed class LoginCommandValidatorTests
{
    [Fact(DisplayName = "Login validator should accept valid input")]
    public void Validate_Should_ReturnValidResult_When_CommandIsValid()
    {
        var validator = new LoginCommandValidator();

        var result = validator.Validate(new LoginCommand("hero@example.com", "StrongPassword1!"));

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "Login validator should reject invalid input")]
    public void Validate_Should_ReturnErrors_When_CommandIsInvalid()
    {
        var validator = new LoginCommandValidator();

        var result = validator.Validate(new LoginCommand("invalid", ""));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(LoginCommand.Email));
        result.Errors.ShouldContain(error => error.PropertyName == nameof(LoginCommand.Password));
    }
}
