using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Register;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Auth.Register;

public sealed class RegisterUserCommandValidatorTests
{
    [Fact(DisplayName = "Register validator should accept valid input")]
    public void Validate_Should_ReturnValidResult_When_CommandIsValid()
    {
        var validator = new RegisterUserCommandValidator();

        var result = validator.Validate(
            new RegisterUserCommand("hero@example.com", "hero", "StrongPassword1!"));

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "Register validator should reject invalid input")]
    public void Validate_Should_ReturnErrors_When_CommandIsInvalid()
    {
        var validator = new RegisterUserCommandValidator();

        var result = validator.Validate(new RegisterUserCommand("invalid", "", ""));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(RegisterUserCommand.Email));
        result.Errors.ShouldContain(error => error.PropertyName == nameof(RegisterUserCommand.UserName));
        result.Errors.ShouldContain(error => error.PropertyName == nameof(RegisterUserCommand.Password));
    }
}
