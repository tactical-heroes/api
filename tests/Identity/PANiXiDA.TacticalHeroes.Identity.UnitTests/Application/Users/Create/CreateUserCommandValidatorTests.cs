using System.Security.Claims;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.Create;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Users.Create;

public sealed class CreateUserCommandValidatorTests
{
    [Fact(DisplayName = "Create user validator should accept valid input")]
    public void Validate_Should_ReturnValidResult_When_CommandIsValid()
    {
        var validator = new CreateUserCommandValidator();

        var result = validator.Validate(
            new CreateUserCommand(
                "hero@example.com",
                "hero",
                "StrongPassword1!",
                true,
                [new Claim("permission", "heroes.read")],
                "Active"));

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "Create user validator should reject invalid input")]
    public void Validate_Should_ReturnErrors_When_CommandIsInvalid()
    {
        var validator = new CreateUserCommandValidator();

        var result = validator.Validate(
            new CreateUserCommand("invalid", "", "", false, [new Claim("", "")], "Deleted"));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(CreateUserCommand.Email));
        result.Errors.ShouldContain(error => error.PropertyName == nameof(CreateUserCommand.UserName));
        result.Errors.ShouldContain(error => error.PropertyName == nameof(CreateUserCommand.Password));
        result.Errors.Count(error =>
            error.PropertyName.StartsWith("Claims[0]", StringComparison.Ordinal)).ShouldBe(2);
        result.Errors.ShouldContain(error => error.PropertyName == nameof(CreateUserCommand.Status));
    }
}
