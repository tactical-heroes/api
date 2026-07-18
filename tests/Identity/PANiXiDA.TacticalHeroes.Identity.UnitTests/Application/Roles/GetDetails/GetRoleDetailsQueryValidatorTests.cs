using PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetDetails;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Roles.GetDetails;

public sealed class GetRoleDetailsQueryValidatorTests
{
    [Fact(DisplayName = "Role details validator should reject an empty role id")]
    public void Validate_Should_ReturnError_When_RoleIdIsEmpty()
    {
        var validator = new GetRoleDetailsQueryValidator();

        var result = validator.Validate(new GetRoleDetailsQuery(Guid.Empty));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(GetRoleDetailsQuery.Id));
    }
}
