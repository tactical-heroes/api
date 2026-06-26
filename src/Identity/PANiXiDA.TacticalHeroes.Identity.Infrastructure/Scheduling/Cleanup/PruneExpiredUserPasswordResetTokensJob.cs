using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserPasswordResetTokens;
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

        var tokens = await dbContext.Set<UserPasswordResetToken>()
            .ToListAsync(cancellationToken);

        var expiredTokens = tokens
            .Where(token => token.ExpiresAtUtc.IsExpired(nowUtc))
            .ToArray();

        if (expiredTokens.Length == 0)
        {
            return;
        }

        dbContext.Set<UserPasswordResetToken>().RemoveRange(expiredTokens);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
