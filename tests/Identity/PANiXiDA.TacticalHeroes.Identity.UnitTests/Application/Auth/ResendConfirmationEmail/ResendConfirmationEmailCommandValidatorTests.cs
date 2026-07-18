using PANiXiDA.TacticalHeroes.Identity.Application.Auth.ResendConfirmationEmail;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Auth.ResendConfirmationEmail;

public sealed class ResendConfirmationEmailCommandValidatorTests
{
    [Fact(DisplayName = "Resend confirmation validator should accept a valid email")]
    public void Validate_Should_ReturnValidResult_When_EmailIsValid()
    {
        var validator = new ResendConfirmationEmailCommandValidator();

        var result = validator.Validate(new ResendConfirmationEmailCommand("hero@example.com"));

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "Resend confirmation validator should reject an invalid email")]
    public void Validate_Should_ReturnError_When_EmailIsInvalid()
    {
        var validator = new ResendConfirmationEmailCommandValidator();

        var result = validator.Validate(new ResendConfirmationEmailCommand("invalid"));

        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(ResendConfirmationEmailCommand.Email));
    }
}
