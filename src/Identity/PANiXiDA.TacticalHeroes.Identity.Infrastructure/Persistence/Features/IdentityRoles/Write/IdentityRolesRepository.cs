using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityRoles;
using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityRoles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.IdentityRoles.Write;

public sealed class IdentityRolesRepository(
    IdentityWriteDbContext dbContext,
    IAggregateTracker aggregateTracker)
    : EfRepository<IdentityWriteDbContext, IdentityRoleId, IdentityRole>(dbContext, aggregateTracker),
    IIdentityRolesRepository
{
    public async Task<IReadOnlyCollection<IdentityRole>> GetByIdsAsync(
        IReadOnlyCollection<IdentityRoleId> roleIds,
        CancellationToken cancellationToken)
    {
        if (roleIds.Count == 0)
        {
            return [];
        }

        return await DbSet
            .Include(role => role.Permissions)
            .Where(role => roleIds.Contains(role.Id))
            .ToArrayAsync(cancellationToken);
    }
}
