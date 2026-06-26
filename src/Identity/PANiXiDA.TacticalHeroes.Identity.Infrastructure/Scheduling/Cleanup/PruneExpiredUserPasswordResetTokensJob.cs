using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserPasswordResetTokens;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserPasswordResetTokens.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;

using Quartz;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Scheduling.Cleanup;

[DisallowConcurrentExecution]
internal sealed class PruneExpiredUserPasswordResetTokensJob(
    IdentityWriteDbContext dbContext,
    TimeProvider timeProvider)
    : IJob
{
    public static readonly JobKey Key = new(nameof(PruneExpiredUserPasswordResetTokensJob));

    public Task Execute(IJobExecutionContext context)
    {
        return ExecuteAsync(context.CancellationToken);
    }

    internal async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var nowUtc = timeProvider.GetUtcNow();
        var nowExpiration = PasswordResetTokenExpiration.Create(nowUtc).Value;

        var users = await dbContext.Set<User>()
            .IgnoreAutoIncludes()
            .Include(user => user.PasswordResetToken)
            .Where(user =>
                user.PasswordResetToken != null &&
                EF.Property<PasswordResetTokenExpiration>(
                    user.PasswordResetToken,
                    nameof(UserPasswordResetToken.ExpiresAtUtc)) < nowExpiration)
            .ToListAsync(cancellationToken);

        if (users.Count == 0)
        {
            return;
        }

        foreach (var user in users)
        {
            dbContext.Entry(user)
                .Reference(storedUser => storedUser.PasswordResetToken)
                .CurrentValue = null;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
