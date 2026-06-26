using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;

using Quartz;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Scheduling.Cleanup;

[DisallowConcurrentExecution]
internal sealed class PruneExpiredUserConfirmationTokensJob(
    IdentityWriteDbContext dbContext,
    TimeProvider timeProvider)
    : IJob
{
    public static readonly JobKey Key = new(nameof(PruneExpiredUserConfirmationTokensJob));

    public Task Execute(IJobExecutionContext context)
    {
        return ExecuteAsync(context.CancellationToken);
    }

    internal async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var nowUtc = timeProvider.GetUtcNow();

        var tokens = await dbContext.UserConfirmationTokens
            .ToListAsync(cancellationToken);

        var expiredTokens = tokens
            .Where(token => token.ExpiresAtUtc.IsExpired(nowUtc))
            .ToArray();

        if (expiredTokens.Length == 0)
        {
            return;
        }

        dbContext.UserConfirmationTokens.RemoveRange(expiredTokens);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
