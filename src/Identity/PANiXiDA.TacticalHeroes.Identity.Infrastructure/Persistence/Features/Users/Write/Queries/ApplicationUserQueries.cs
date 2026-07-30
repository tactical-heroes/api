using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.Queries;

internal static class ApplicationUserQueries
{
    internal static IQueryable<ApplicationUser> WithAuthorizationGraph(
        this IQueryable<ApplicationUser> query)
    {
        return query
            .Include(navigationPropertyPath: user => user.Claims)
            .Include(navigationPropertyPath: user => user.Roles)
                .ThenInclude(navigationPropertyPath: userRole => userRole.Role)
                    .ThenInclude(navigationPropertyPath: role => role!.Claims)
            .AsSingleQuery();
    }
}
