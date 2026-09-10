using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Entities.RoleClaims;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Entities.RoleClaims.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Domain.Roles;

public sealed class RoleTests
{
    [Fact(DisplayName = "Create should restore persisted role state when persisted values are valid")]
    public void Create_Should_RestoreState_When_PersistedValuesAreValid()
    {
        var id = Guid.CreateVersion7();

        var result = Role.Create(
            RoleId.Create(id).Value,
            RoleName.Create(" ADMIN ").Value,
            [RoleClaim.Create(ClaimType.Create("permission").Value, ClaimValue.Create("heroes.manage").Value)]);

        result.Id.Value.ShouldBe(id);
        result.Name.Value.ShouldBe("admin");
        var claim = result.Claims.ShouldHaveSingleItem();
        claim.Type.Value.ShouldBe("permission");
        claim.Value.Value.ShouldBe("heroes.manage");
    }

    [Fact(DisplayName = "Grant claim should add a valid claim only once when claim is valid")]
    public void GrantClaim_Should_AddClaimOnce_When_ClaimIsValid()
    {
        var role = CreateRole();

        role.GrantClaim(RoleClaim.Create(ClaimType.Create(value: "permission").Value, ClaimValue.Create(value: "heroes.manage").Value));
        role.GrantClaim(RoleClaim.Create(ClaimType.Create(value: "permission").Value, ClaimValue.Create(value: "heroes.manage").Value));

        role.Claims.ShouldHaveSingleItem();
    }

    [Fact(DisplayName = "Revoke claim should remove the matching claim when claim exists")]
    public void RevokeClaim_Should_RemoveClaim_When_ClaimExists()
    {
        var role = CreateRole();
        role.GrantClaim(RoleClaim.Create(ClaimType.Create(value: "permission").Value, ClaimValue.Create(value: "heroes.read").Value));
        role.GrantClaim(RoleClaim.Create(ClaimType.Create(value: "permission").Value, ClaimValue.Create(value: "heroes.manage").Value));

        role.RevokeClaim(ClaimType.Create(value: "permission").Value, ClaimValue.Create(value: "heroes.read").Value);

        var claim = role.Claims.ShouldHaveSingleItem();
        claim.Value.Value.ShouldBe("heroes.manage");
    }

    private static Role CreateRole()
    {
        return Role.Create(RoleId.New(), RoleName.Create("admin").Value, []);
    }
}
