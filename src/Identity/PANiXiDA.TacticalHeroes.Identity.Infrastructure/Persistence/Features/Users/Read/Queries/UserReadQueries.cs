using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.Queries;

internal static class UserReadQueries
{
    internal static IQueryable<UserReadDbModel> WithAuthorizationGraph(
        this IQueryable<UserReadDbModel> query)
    {
        return query
            .Include(user => user.Claims)
            .Include(user => user.Roles)
                .ThenInclude(userRole => userRole.Role)
                    .ThenInclude(role => role!.Claims)
            .AsSingleQuery();
    }
}
