using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.Queries;

internal static class ApplicationUserQueries
{
    internal static IQueryable<ApplicationUser> WithAuthorizationGraph(
        this IQueryable<ApplicationUser> query)
    {
        return query
            .Include(user => user.Claims)
            .Include(user => user.Roles)
                .ThenInclude(userRole => userRole.Role)
                    .ThenInclude(role => role!.Claims)
            .AsSingleQuery();
    }
}
