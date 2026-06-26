using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserPasswordResetTokens;
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

        var users = await dbContext.Users
            .IgnoreAutoIncludes()
            .Include(user => user.PasswordResetToken)
            .Where(user =>
                user.PasswordResetToken != null &&
                EF.Property<DateTimeOffset>(
                    user.PasswordResetToken,
                    nameof(UserPasswordResetToken.ExpiresAtUtc)) < nowUtc)
            .ToListAsync(context.CancellationToken);

        foreach (var user in users)
        {
            user.RemoveExpiredPasswordResetToken(nowUtc);
        }

        await dbContext.SaveChangesAsync(context.CancellationToken);
    }
}
