using PANiXiDA.TacticalHeroes.Identity.Application.Auth.ConfirmEmail;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Auth.ConfirmEmail;

public sealed class ConfirmEmailCommandValidatorTests
{
    [Fact(DisplayName = "Confirm email validator should accept valid input")]
    public void Validate_Should_ReturnValidResult_When_CommandIsValid()
    {
        var validator = new ConfirmEmailCommandValidator();

        var result = validator.Validate(
            new ConfirmEmailCommand(Guid.CreateVersion7(), "confirmation-token"));

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "Confirm email validator should reject invalid input")]
    public void Validate_Should_ReturnErrors_When_CommandIsInvalid()
    {
        var validator = new ConfirmEmailCommandValidator();

        var result = validator.Validate(new ConfirmEmailCommand(Guid.Empty, ""));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(ConfirmEmailCommand.UserId));
        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(ConfirmEmailCommand.EmailConfirmationToken));
    }
}
