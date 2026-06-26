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
        var users = await dbContext.Users
            .IgnoreAutoIncludes()
            .ToListAsync(cancellationToken);

        var staleUnconfirmedUsers = users
            .Where(user =>
                !user.ConfirmationStatus.IsConfirmed &&
                dbContext.Entry(user)
                    .Property<DateTime>(EfConstants.CreatedAt)
                    .CurrentValue < deleteBeforeUtc)
            .ToArray();

        if (staleUnconfirmedUsers.Length == 0)
        {
            return;
        }

        dbContext.Users.RemoveRange(staleUnconfirmedUsers);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
