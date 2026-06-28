using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Read.Filters;

internal static class RolesFilter
{
    public static IQueryable<RoleReadDbModel> Apply(
        IQueryable<RoleReadDbModel> query)
    {
        return query;
    }
}
