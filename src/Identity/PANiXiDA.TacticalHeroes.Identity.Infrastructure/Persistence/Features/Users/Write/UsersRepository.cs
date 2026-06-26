using Microsoft.EntityFrameworkCore;

using PANiXiDA.Core.SpecificationPattern.Abstractions;
using PANiXiDA.Core.SpecificationPattern.Extensions;

using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write;

public sealed class UsersRepository(
    IdentityWriteDbContext dbContext,
    IAggregateTracker aggregateTracker)
    : EfRepository<IdentityWriteDbContext, UserId, User>(dbContext, aggregateTracker),
    IUsersRepository
{
    public Task<User?> GetBySpecificationAsync(
        ISpecification<User> specification,
        CancellationToken cancellationToken)
    {
        return DbSet
            .Where(specification)
            .SingleOrDefaultAsync(cancellationToken);
    }
}
