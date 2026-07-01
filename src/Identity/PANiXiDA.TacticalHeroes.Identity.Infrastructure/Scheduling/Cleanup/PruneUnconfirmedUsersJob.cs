using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;
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

    public Task Execute(IJobExecutionContext context)
    {
        return ExecuteAsync(context.CancellationToken);
    }

    internal async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var deleteBeforeUtc = timeProvider
            .GetUtcNow()
            .Subtract(options.Value.UnconfirmedUserRetention)
            .UtcDateTime;

        await dbContext.Set<ApplicationUser>()
            .Where(user =>
                !user.EmailConfirmed &&
                user.CreatedAt < deleteBeforeUtc)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
