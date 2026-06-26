using Microsoft.EntityFrameworkCore;

using PANiXiDA.Core.SpecificationPattern.Abstractions;
using PANiXiDA.Core.SpecificationPattern.Extensions;

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
    public async Task<IReadOnlyCollection<Role>> GetBySpecificationAsync(
        ISpecification<Role> specification,
        CancellationToken cancellationToken)
    {
        return await DbSet
            .Include(role => role.Claims)
            .Where(specification)
            .ToArrayAsync(cancellationToken);
    }
}
