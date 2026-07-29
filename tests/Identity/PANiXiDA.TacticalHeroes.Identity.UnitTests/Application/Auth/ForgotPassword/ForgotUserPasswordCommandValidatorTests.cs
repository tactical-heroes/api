using PANiXiDA.TacticalHeroes.Identity.Application.Auth.ForgotPassword;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Auth.ForgotPassword;

public sealed class ForgotUserPasswordCommandValidatorTests
{
    [Fact(DisplayName = "Forgot password validator should accept a valid email when email is valid")]
    public void Validate_Should_ReturnValidResult_When_EmailIsValid()
    {
        var validator = new ForgotUserPasswordCommandValidator();

        var result = validator.Validate(new ForgotUserPasswordCommand("hero@example.com"));

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "Forgot password validator should reject an invalid email when email is invalid")]
    public void Validate_Should_ReturnError_When_EmailIsInvalid()
    {
        var validator = new ForgotUserPasswordCommandValidator();

        var result = validator.Validate(new ForgotUserPasswordCommand("invalid"));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(ForgotUserPasswordCommand.Email));
    }
}
