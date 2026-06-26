using Microsoft.EntityFrameworkCore;

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

    public async Task Execute(IJobExecutionContext context)
    {
        var nowUtc = timeProvider.GetUtcNow();

        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
             DELETE FROM identity_user_password_reset_tokens
             WHERE expires_at_utc < {nowUtc}
             """,
            context.CancellationToken);
    }
}
