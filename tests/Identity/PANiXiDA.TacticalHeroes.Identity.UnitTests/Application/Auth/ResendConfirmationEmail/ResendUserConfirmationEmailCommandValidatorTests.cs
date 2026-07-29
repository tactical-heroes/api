using PANiXiDA.TacticalHeroes.Identity.Application.Auth.ResendConfirmationEmail;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Auth.ResendConfirmationEmail;

public sealed class ResendUserConfirmationEmailCommandValidatorTests
{
    [Fact(DisplayName = "Resend confirmation validator should accept a valid email when email is valid")]
    public void Validate_Should_ReturnValidResult_When_EmailIsValid()
    {
        var validator = new ResendUserConfirmationEmailCommandValidator();

        var result = validator.Validate(new ResendUserConfirmationEmailCommand("hero@example.com"));

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "Resend confirmation validator should reject an invalid email when email is invalid")]
    public void Validate_Should_ReturnError_When_EmailIsInvalid()
    {
        var validator = new ResendUserConfirmationEmailCommandValidator();

        var result = validator.Validate(new ResendUserConfirmationEmailCommand("invalid"));

        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(ResendUserConfirmationEmailCommand.Email));
    }
}
