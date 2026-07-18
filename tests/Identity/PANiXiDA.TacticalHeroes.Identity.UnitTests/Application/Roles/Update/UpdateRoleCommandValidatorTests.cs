using System.Security.Claims;

using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Update;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Roles.Update;

public sealed class UpdateRoleCommandValidatorTests
{
    [Fact(DisplayName = "Update role validator should accept valid input")]
    public void Validate_Should_ReturnValidResult_When_CommandIsValid()
    {
        var validator = new UpdateRoleCommandValidator();

        var result = validator.Validate(
            new UpdateRoleCommand(
                Guid.CreateVersion7(),
                "admin",
                [new Claim("permission", "heroes.manage")]));

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "Update role validator should reject invalid input")]
    public void Validate_Should_ReturnErrors_When_CommandIsInvalid()
    {
        var validator = new UpdateRoleCommandValidator();

        var result = validator.Validate(
            new UpdateRoleCommand(Guid.Empty, "", [new Claim("", "")]));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(UpdateRoleCommand.Id));
        result.Errors.ShouldContain(error => error.PropertyName == nameof(UpdateRoleCommand.Name));
        result.Errors.Count(error =>
            error.PropertyName.StartsWith("Claims[0]", StringComparison.Ordinal)).ShouldBe(2);
    }
}
