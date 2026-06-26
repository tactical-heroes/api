using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;

using Quartz;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Users.Cleanup;

[DisallowConcurrentExecution]
internal sealed class PruneExpiredUserConfirmationTokensJob(
    IdentityWriteDbContext dbContext,
    TimeProvider timeProvider)
    : IJob
{
    public static readonly JobKey Key = new(nameof(PruneExpiredUserConfirmationTokensJob));

    public async Task Execute(IJobExecutionContext context)
    {
        var nowUtc = timeProvider.GetUtcNow();

        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
             DELETE FROM identity_user_confirmation_tokens
             WHERE expires_at_utc < {nowUtc}
             """,
            context.CancellationToken);
    }
}
