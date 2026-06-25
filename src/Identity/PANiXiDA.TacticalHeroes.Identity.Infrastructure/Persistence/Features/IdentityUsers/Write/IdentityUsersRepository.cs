using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers;
using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.IdentityUsers.Write;

public sealed class IdentityUsersRepository(
    IdentityWriteDbContext dbContext,
    IAggregateTracker aggregateTracker)
    : EfRepository<IdentityWriteDbContext, IdentityUserId, IdentityUser>(dbContext, aggregateTracker),
    IIdentityUsersRepository
{
    public Task<IdentityUser?> GetByEmailAsync(
        Email email,
        CancellationToken cancellationToken)
    {
        return DbSet
            .SingleOrDefaultAsync(user => user.Email == email, cancellationToken);
    }
}
