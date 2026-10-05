using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Entities.RoleClaims;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Entities.RoleClaims.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Roles;

internal static class IntegrationTestData
{
    internal static Role CreateRole(
        Guid id,
        string name,
        IEnumerable<(string Type, string Value)> claims)
    {
        return Role.Create(
            id: RoleId.Create(value: id).Value,
            name: RoleName.Create(value: name).Value,
            claims: claims.Select(claim => RoleClaim.Create(
                type: ClaimType.Create(value: claim.Type).Value,
                value: ClaimValue.Create(value: claim.Value).Value)));
    }
}
