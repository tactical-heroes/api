using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Scheduling.Options.IdentityCleanup;

using Quartz;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Scheduling.Cleanup;

[DisallowConcurrentExecution]
internal sealed class PruneUnconfirmedUsersJob(
    IdentityWriteDbContext dbContext,
    TimeProvider timeProvider,
    IOptions<IdentityCleanupOptions> options)
    : IJob
{
    public static readonly JobKey Key = new(name: nameof(PruneUnconfirmedUsersJob));

    ValueTask IJob.Execute(IJobExecutionContext context, CancellationToken cancellationToken)
    {
        return new ValueTask(task: ExecuteAsync(cancellationToken: cancellationToken));
    }

    internal async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var cleanupOptions = options.Value;

        if (!cleanupOptions.PruneUnconfirmedUsersEnabled)
        {
            return;
        }

        var deleteBeforeUtc = timeProvider
            .GetUtcNow()
            .Subtract(value: cleanupOptions.UnconfirmedUserRetention)
            .UtcDateTime;

        await dbContext.Set<ApplicationUser>()
            .Where(predicate: user =>
                !user.EmailConfirmed &&
                user.CreatedAt < deleteBeforeUtc)
            .ExecuteDeleteAsync(cancellationToken: cancellationToken);
    }
}
