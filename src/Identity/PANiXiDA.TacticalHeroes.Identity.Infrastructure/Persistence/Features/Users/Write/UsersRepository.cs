using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write;

public sealed class UsersRepository(
    IdentityWriteDbContext dbContext,
    IAggregateTracker aggregateTracker)
    : EfRepository<IdentityWriteDbContext, UserId, User>(dbContext, aggregateTracker),
    IUsersRepository
{
    public Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(email);

        if (emailResult.IsFailure)
        {
            return Task.FromResult<User?>(null);
        }

        return DbSet
            .SingleOrDefaultAsync(user => user.Email == emailResult.Value, cancellationToken);
    }
}
