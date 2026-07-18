using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Entities.RoleClaims;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.Mappers;

internal static class ApplicationRoleMapper
{
    public static ApplicationRole ToDbModel(
        Role role,
        DateTime createdAt,
        DateTime updatedAt)
    {
        var dbModel = new ApplicationRole
        {
            Id = role.Id.Value,
            CreatedAt = createdAt,
            Claims = ToClaimDbModels(claims: role.Claims)
        };

        MapToDbModel(
            role: role,
            dbModel: dbModel,
            updatedAt: updatedAt);

        return dbModel;
    }

    public static void MapToDbModel(
        Role role,
        ApplicationRole dbModel,
        DateTime updatedAt)
    {
        dbModel.Name = role.Name.Value;
        dbModel.UpdatedAt = updatedAt;
    }

    public static List<ApplicationRoleClaim> ToClaimDbModels(
        IEnumerable<RoleClaim> claims)
    {
        return
        [
            .. claims.Select(claim => new ApplicationRoleClaim
            {
                ClaimType = claim.Type.Value,
                ClaimValue = claim.Value.Value
            })
        ];
    }

    public static Result<Role> ToDomain(ApplicationRole role)
    {
        return Role.Create(
            id: role.Id,
            name: role.Name!,
            claims: role.Claims.Select(claim => (claim.ClaimType!, claim.ClaimValue!)));
    }
}
