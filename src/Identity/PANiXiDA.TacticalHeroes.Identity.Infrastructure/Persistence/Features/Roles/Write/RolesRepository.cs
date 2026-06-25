using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write;

public sealed class RolesRepository(
    IdentityWriteDbContext dbContext,
    IAggregateTracker aggregateTracker)
    : EfRepository<IdentityWriteDbContext, RoleId, Role>(dbContext, aggregateTracker),
    IRolesRepository
{
    public async Task<IReadOnlyCollection<Role>> GetByIdsAsync(
        IReadOnlyCollection<RoleId> roleIds,
        CancellationToken cancellationToken)
    {
        if (roleIds.Count == 0)
        {
            return [];
        }

        return await DbSet
            .Include(role => role.Claims)
            .Where(role => roleIds.Contains(role.Id))
            .ToArrayAsync(cancellationToken);
    }
}
