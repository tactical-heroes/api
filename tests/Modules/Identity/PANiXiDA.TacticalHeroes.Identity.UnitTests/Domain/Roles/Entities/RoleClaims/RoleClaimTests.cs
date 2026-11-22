using PANiXiDA.TacticalHeroes.Modules.Identity.Domain.Roles.Entities.RoleClaims;
using PANiXiDA.TacticalHeroes.Modules.Identity.Domain.Roles.Entities.RoleClaims.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.UnitTests.Domain.Roles.Entities.RoleClaims;

public sealed class RoleClaimTests
{
    [Fact(DisplayName = "Create should build a role claim from valid values when values are valid")]
    public void Create_Should_ReturnRoleClaim_When_ValuesAreValid()
    {
        var result = RoleClaim.Create(
            ClaimType.Create(value: " permission ").Value,
            ClaimValue.Create(value: " heroes.manage ").Value);

        result.Id.Value.ShouldNotBe(Guid.Empty);
        result.Type.Value.ShouldBe("permission");
        result.Value.Value.ShouldBe("heroes.manage");
    }
}
