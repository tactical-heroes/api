using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;

using Quartz;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Users.Cleanup;

[DisallowConcurrentExecution]
internal sealed class PruneUnconfirmedUsersJob(
    IdentityWriteDbContext dbContext,
    TimeProvider timeProvider,
    IOptions<IdentityCleanupOptions> options)
    : IJob
{
    public static readonly JobKey Key = new(nameof(PruneUnconfirmedUsersJob));

    public async Task Execute(IJobExecutionContext context)
    {
        var deleteBeforeUtc = timeProvider
            .GetUtcNow()
            .Subtract(options.Value.UnconfirmedUserRetention)
            .UtcDateTime;

        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
             DELETE FROM identity_users
             WHERE is_confirmed = false
             AND created_at < {deleteBeforeUtc}
             """,
            context.CancellationToken);
    }
}
