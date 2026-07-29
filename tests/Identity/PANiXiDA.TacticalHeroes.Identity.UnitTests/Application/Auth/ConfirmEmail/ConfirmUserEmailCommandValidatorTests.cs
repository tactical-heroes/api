using PANiXiDA.TacticalHeroes.Identity.Application.Auth.ConfirmEmail;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Auth.ConfirmEmail;

public sealed class ConfirmUserEmailCommandValidatorTests
{
    [Fact(DisplayName = "Confirm email validator should accept valid input when command is valid")]
    public void Validate_Should_ReturnValidResult_When_CommandIsValid()
    {
        var validator = new ConfirmUserEmailCommandValidator();

        var result = validator.Validate(
            new ConfirmUserEmailCommand(Guid.CreateVersion7(), "confirmation-token"));

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "Confirm email validator should reject invalid input when command is invalid")]
    public void Validate_Should_ReturnErrors_When_CommandIsInvalid()
    {
        var validator = new ConfirmUserEmailCommandValidator();

        var result = validator.Validate(new ConfirmUserEmailCommand(Guid.Empty, ""));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(ConfirmUserEmailCommand.UserId));
        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(ConfirmUserEmailCommand.EmailConfirmationToken));
    }
}
