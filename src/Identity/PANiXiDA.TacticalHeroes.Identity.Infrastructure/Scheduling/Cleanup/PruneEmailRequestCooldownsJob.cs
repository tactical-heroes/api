using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;

using Quartz;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Scheduling.Cleanup;

[DisallowConcurrentExecution]
internal sealed class PruneEmailRequestCooldownsJob(IdentityWriteDbContext dbContext) : IJob
{
    public static readonly JobKey Key = new(nameof(PruneEmailRequestCooldownsJob));

    public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken)
    {
        await dbContext.Database.ExecuteSqlRawAsync(
            "DELETE FROM identity.email_request_cooldowns WHERE expires_at <= clock_timestamp()",
            cancellationToken);
    }
}
