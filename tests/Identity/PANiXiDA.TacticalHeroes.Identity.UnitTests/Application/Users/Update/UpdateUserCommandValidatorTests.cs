using System.Security.Claims;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.Update;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Users.Update;

public sealed class UpdateUserCommandValidatorTests
{
    [Fact(DisplayName = "Update user validator should accept valid input")]
    public void Validate_Should_ReturnValidResult_When_CommandIsValid()
    {
        var validator = new UpdateUserCommandValidator();

        var result = validator.Validate(
            new UpdateUserCommand(
                Guid.CreateVersion7(),
                "hero@example.com",
                "hero",
                true,
                [new Claim("permission", "heroes.manage")],
                "Blocked"));

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "Update user validator should reject invalid input")]
    public void Validate_Should_ReturnErrors_When_CommandIsInvalid()
    {
        var validator = new UpdateUserCommandValidator();

        var result = validator.Validate(
            new UpdateUserCommand(
                Guid.Empty,
                "invalid",
                "",
                false,
                [new Claim("", "")],
                "Deleted"));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(UpdateUserCommand.Id));
        result.Errors.ShouldContain(error => error.PropertyName == nameof(UpdateUserCommand.Email));
        result.Errors.ShouldContain(error => error.PropertyName == nameof(UpdateUserCommand.UserName));
        result.Errors.Count(error =>
            error.PropertyName.StartsWith("Claims[0]", StringComparison.Ordinal)).ShouldBe(2);
        result.Errors.ShouldContain(error => error.PropertyName == nameof(UpdateUserCommand.Status));
    }
}
