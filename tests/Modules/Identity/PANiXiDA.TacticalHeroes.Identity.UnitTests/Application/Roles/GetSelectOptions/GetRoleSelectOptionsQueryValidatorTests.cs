using PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetSelectOptions;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Roles.GetSelectOptions;

public sealed class GetRoleSelectOptionsQueryValidatorTests
{
    [Fact(DisplayName = "Role select options validator should accept a query when query is valid")]
    public void Validate_Should_ReturnSuccess_When_QueryIsValid()
    {
        var validator = new GetRoleSelectOptionsQueryValidator();

        var result = validator.Validate(new GetRoleSelectOptionsQuery());

        result.IsValid.ShouldBeTrue();
    }
}
