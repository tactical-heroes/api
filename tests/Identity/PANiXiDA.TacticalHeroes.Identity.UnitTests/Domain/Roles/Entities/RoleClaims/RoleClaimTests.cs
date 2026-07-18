using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Entities.RoleClaims;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Domain.Roles.Entities.RoleClaims;

public sealed class RoleClaimTests
{
    [Fact(DisplayName = "Create should build a role claim from valid values")]
    public void Create_Should_ReturnRoleClaim_When_ValuesAreValid()
    {
        var result = RoleClaim.Create(" permission ", " heroes.manage ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.Value.ShouldNotBe(Guid.Empty);
        result.Value.Type.Value.ShouldBe("permission");
        result.Value.Value.Value.ShouldBe("heroes.manage");
    }

    [Fact(DisplayName = "Create should reject an invalid role claim type")]
    public void Create_Should_ReturnValidationFailure_When_TypeIsInvalid()
    {
        var result = RoleClaim.Create("", "heroes.manage");

        result.ShouldHaveSingleError(ErrorType.Validation, "Claim type cannot be empty.");
    }
}
