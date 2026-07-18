using PANiXiDA.TacticalHeroes.Identity.Application.Auth.ForgotPassword;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Auth.ForgotPassword;

public sealed class ForgotPasswordCommandValidatorTests
{
    [Fact(DisplayName = "Forgot password validator should accept a valid email")]
    public void Validate_Should_ReturnValidResult_When_EmailIsValid()
    {
        var validator = new ForgotPasswordCommandValidator();

        var result = validator.Validate(new ForgotPasswordCommand("hero@example.com"));

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "Forgot password validator should reject an invalid email")]
    public void Validate_Should_ReturnError_When_EmailIsInvalid()
    {
        var validator = new ForgotPasswordCommandValidator();

        var result = validator.Validate(new ForgotPasswordCommand("invalid"));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(ForgotPasswordCommand.Email));
    }
}
