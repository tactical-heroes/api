using PANiXiDA.TacticalHeroes.Identity.Application.Auth.ChangePassword;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Auth.ChangePassword;

public sealed class ChangePasswordCommandValidatorTests
{
    [Fact(DisplayName = "Change password validator should accept valid input")]
    public void Validate_Should_ReturnValidResult_When_CommandIsValid()
    {
        var validator = new ChangePasswordCommandValidator();

        var result = validator.Validate(
            new ChangePasswordCommand(Guid.CreateVersion7(), "CurrentPassword1!", "NewPassword1!"));

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "Change password validator should reject invalid input")]
    public void Validate_Should_ReturnErrors_When_CommandIsInvalid()
    {
        var validator = new ChangePasswordCommandValidator();

        var result = validator.Validate(new ChangePasswordCommand(Guid.Empty, "", ""));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(ChangePasswordCommand.UserId));
        result.Errors.ShouldContain(error => error.PropertyName == nameof(ChangePasswordCommand.CurrentPassword));
        result.Errors.ShouldContain(error => error.PropertyName == nameof(ChangePasswordCommand.NewPassword));
    }
}
