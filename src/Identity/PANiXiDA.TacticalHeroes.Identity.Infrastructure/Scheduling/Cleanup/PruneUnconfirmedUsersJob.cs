using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Scheduling.Options;

using Quartz;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Scheduling.Cleanup;

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

        await dbContext.Users
            .Where(user =>
                !EF.Property<bool>(user, "IsConfirmed") &&
                EF.Property<DateTime>(user, "CreatedAt") < deleteBeforeUtc)
            .ExecuteDeleteAsync(context.CancellationToken);
    }
}
