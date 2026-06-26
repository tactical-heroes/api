using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
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

    public Task Execute(IJobExecutionContext context)
    {
        return ExecuteAsync(context.CancellationToken);
    }

    internal async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var nowUtc = timeProvider.GetUtcNow();

        var users = await dbContext.Set<User>()
            .IgnoreAutoIncludes()
            .Include(user => user.PasswordResetToken)
            .Where(user => user.PasswordResetToken != null)
            .ToListAsync(cancellationToken);

        var expiredTokenUsers = users
            .Where(user => user.PasswordResetToken!.ExpiresAtUtc.IsExpired(nowUtc))
            .ToArray();

        if (expiredTokenUsers.Length == 0)
        {
            return;
        }

        foreach (var user in expiredTokenUsers)
        {
            dbContext.Entry(user)
                .Reference(storedUser => storedUser.PasswordResetToken)
                .CurrentValue = null;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
