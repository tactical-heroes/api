using PANiXiDA.TacticalHeroes.Identity.Application.Auth.ResetPassword;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Auth.ResetPassword;

public sealed class ResetPasswordCommandValidatorTests
{
    [Fact(DisplayName = "Reset password validator should accept valid input")]
    public void Validate_Should_ReturnValidResult_When_CommandIsValid()
    {
        var validator = new ResetPasswordCommandValidator();

        var result = validator.Validate(
            new ResetPasswordCommand(Guid.CreateVersion7(), "reset-token", "NewPassword1!"));

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "Reset password validator should reject invalid input")]
    public void Validate_Should_ReturnErrors_When_CommandIsInvalid()
    {
        var validator = new ResetPasswordCommandValidator();

        var result = validator.Validate(new ResetPasswordCommand(Guid.Empty, "", ""));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(ResetPasswordCommand.UserId));
        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(ResetPasswordCommand.PasswordResetToken));
        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(ResetPasswordCommand.NewPassword));
    }
}
