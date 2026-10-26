using PANiXiDA.TacticalHeroes.Identity.Application.Claims.GetPermissions;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Claims.GetPermissions;

public sealed class GetPermissionsQueryValidatorTests
{
    [Fact(DisplayName = "Permissions validator should accept a query when query is valid")]
    public void Validate_Should_ReturnSuccess_When_QueryIsValid()
    {
        var validator = new GetPermissionsQueryValidator();

        var result = validator.Validate(new GetPermissionsQuery());

        result.IsValid.ShouldBeTrue();
    }
}
