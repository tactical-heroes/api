using PANiXiDA.TacticalHeroes.Identity.Application.Auth.ResendConfirmationEmail;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Auth.ResendConfirmationEmail;

public sealed class ResendConfirmationEmailCommandValidatorTests
{
    [Fact(DisplayName = "Resend confirmation validator should accept a valid email when email is valid")]
    public void Validate_Should_ReturnValidResult_When_EmailIsValid()
    {
        var validator = new ResendConfirmationEmailCommandValidator();

        var result = validator.Validate(new ResendConfirmationEmailCommand("hero@example.com"));

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "Resend confirmation validator should reject an invalid email when email is invalid")]
    public void Validate_Should_ReturnError_When_EmailIsInvalid()
    {
        var validator = new ResendConfirmationEmailCommandValidator();

        var result = validator.Validate(new ResendConfirmationEmailCommand("invalid"));

        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(ResendConfirmationEmailCommand.Email));
    }
}
