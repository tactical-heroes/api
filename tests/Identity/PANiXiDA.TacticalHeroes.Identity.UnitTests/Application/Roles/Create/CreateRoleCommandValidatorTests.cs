using System.Security.Claims;

using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Create;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Roles.Create;

public sealed class CreateRoleCommandValidatorTests
{
    [Fact(DisplayName = "Create role validator should accept valid input")]
    public void Validate_Should_ReturnValidResult_When_CommandIsValid()
    {
        var validator = new CreateRoleCommandValidator();

        var result = validator.Validate(
            new CreateRoleCommand("admin", [new Claim("permission", "heroes.manage")]));

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "Create role validator should reject invalid input")]
    public void Validate_Should_ReturnErrors_When_CommandIsInvalid()
    {
        var validator = new CreateRoleCommandValidator();

        var result = validator.Validate(
            new CreateRoleCommand("", [new Claim("", "")]));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(CreateRoleCommand.Name));
        result.Errors.Count(error =>
            error.PropertyName.StartsWith("Claims[0]", StringComparison.Ordinal)).ShouldBe(2);
    }
}
