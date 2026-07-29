using PANiXiDA.TacticalHeroes.Identity.Application.Users.ChangePassword;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Users.ChangePassword;

public sealed class ChangeUserPasswordCommandValidatorTests
{
    [Fact(DisplayName = "Change password validator should accept valid input when command is valid")]
    public void Validate_Should_ReturnValidResult_When_CommandIsValid()
    {
        var validator = new ChangeUserPasswordCommandValidator();

        var result = validator.Validate(
            new ChangeUserPasswordCommand(Guid.CreateVersion7(), "CurrentPassword1!", "NewPassword1!"));

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "Change password validator should reject invalid input when command is invalid")]
    public void Validate_Should_ReturnErrors_When_CommandIsInvalid()
    {
        var validator = new ChangeUserPasswordCommandValidator();

        var result = validator.Validate(new ChangeUserPasswordCommand(Guid.Empty, "", ""));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(ChangeUserPasswordCommand.UserId));
        result.Errors.ShouldContain(error => error.PropertyName == nameof(ChangeUserPasswordCommand.CurrentPassword));
        result.Errors.ShouldContain(error => error.PropertyName == nameof(ChangeUserPasswordCommand.NewPassword));
    }
}
