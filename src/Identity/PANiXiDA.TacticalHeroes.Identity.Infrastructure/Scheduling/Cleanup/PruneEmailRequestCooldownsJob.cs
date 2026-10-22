using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;

using Quartz;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Scheduling.Cleanup;

[DisallowConcurrentExecution]
internal sealed class PruneEmailRequestCooldownsJob(IdentityWriteDbContext dbContext) : IJob
{
    public static readonly JobKey Key = new(nameof(PruneEmailRequestCooldownsJob));

    ValueTask IJob.Execute(IJobExecutionContext context, CancellationToken cancellationToken)
    {
        return new ValueTask(ExecuteAsync(cancellationToken));
    }

    internal async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await dbContext.Database.ExecuteSqlRawAsync(
            "DELETE FROM identity.email_request_cooldowns WHERE expires_at <= clock_timestamp()",
            cancellationToken);
    }
}
