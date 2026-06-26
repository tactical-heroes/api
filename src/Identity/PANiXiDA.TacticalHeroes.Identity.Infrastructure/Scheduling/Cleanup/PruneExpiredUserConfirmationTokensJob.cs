using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserConfirmationTokens;
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

    public async Task Execute(IJobExecutionContext context)
    {
        var nowUtc = timeProvider.GetUtcNow();

        var users = await dbContext.Users
            .IgnoreAutoIncludes()
            .Include(user => user.ConfirmationToken)
            .Where(user =>
                user.ConfirmationToken != null &&
                EF.Property<DateTimeOffset>(
                    user.ConfirmationToken,
                    nameof(UserConfirmationToken.ExpiresAtUtc)) < nowUtc)
            .ToListAsync(context.CancellationToken);

        foreach (var user in users)
        {
            user.RemoveExpiredConfirmationToken(nowUtc);
        }

        await dbContext.SaveChangesAsync(context.CancellationToken);
    }
}
