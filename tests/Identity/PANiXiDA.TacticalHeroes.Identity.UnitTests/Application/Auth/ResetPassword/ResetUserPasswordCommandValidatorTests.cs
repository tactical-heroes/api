using PANiXiDA.TacticalHeroes.Identity.Application.Auth.ResetPassword;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Auth.ResetPassword;

public sealed class ResetUserPasswordCommandValidatorTests
{
    [Fact(DisplayName = "Reset password validator should accept valid input when command is valid")]
    public void Validate_Should_ReturnValidResult_When_CommandIsValid()
    {
        var validator = new ResetUserPasswordCommandValidator();

        var result = validator.Validate(
            new ResetUserPasswordCommand(Guid.CreateVersion7(), "reset-token", "NewPassword1!"));

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "Reset password validator should reject invalid input when command is invalid")]
    public void Validate_Should_ReturnErrors_When_CommandIsInvalid()
    {
        var validator = new ResetUserPasswordCommandValidator();

        var result = validator.Validate(new ResetUserPasswordCommand(Guid.Empty, "", ""));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(ResetUserPasswordCommand.UserId));
        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(ResetUserPasswordCommand.PasswordResetToken));
        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(ResetUserPasswordCommand.NewPassword));
    }
}
