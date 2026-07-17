using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Scheduling.Cleanup;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Scheduling.Options;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.Scheduling;

public sealed class IdentityCleanupJobTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    private const string Password = "StrongPassword1!";

    private static readonly DateTimeOffset NowUtc = new(
        year: 2026,
        month: 6,
        day: 26,
        hour: 12,
        minute: 0,
        second: 0,
        offset: TimeSpan.Zero);

    [Fact(DisplayName = "Prune unconfirmed users should delete only stale unconfirmed users")]
    public async Task PruneUnconfirmedUsersJob_Should_DeleteOnlyStaleUnconfirmedUsers()
    {
        var staleUnconfirmedUser = CreateUser(email: "stale-unconfirmed@example.com");
        var recentUnconfirmedUser = CreateUser(email: "recent-unconfirmed@example.com");
        var staleConfirmedUser = CreateUser(email: "stale-confirmed@example.com");

        staleConfirmedUser.EmailConfirmed = true;

        await using (var scope = Fixture.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            (await userManager.CreateAsync(user: staleUnconfirmedUser, password: Password)).Succeeded.ShouldBeTrue();
            (await userManager.CreateAsync(user: recentUnconfirmedUser, password: Password)).Succeeded.ShouldBeTrue();
            (await userManager.CreateAsync(user: staleConfirmedUser, password: Password)).Succeeded.ShouldBeTrue();

            await SetCreatedAtAsync(dbContext, staleUnconfirmedUser, NowUtc.AddDays(-8));
            await SetCreatedAtAsync(dbContext, recentUnconfirmedUser, NowUtc.AddDays(-1));
            await SetCreatedAtAsync(dbContext, staleConfirmedUser, NowUtc.AddDays(-8));
        }

        await using (var scope = Fixture.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
            var job = new PruneUnconfirmedUsersJob(
                dbContext: dbContext,
                timeProvider: new FrozenTimeProvider(utcNow: NowUtc),
                options: Options.Create(options: new IdentityCleanupOptions
                {
                    UnconfirmedUserRetention = TimeSpan.FromDays(days: 7)
                }));

            await job.ExecuteAsync(TestContext.Current.CancellationToken);
        }

        await using (var scope = Fixture.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
            var remainingUserIds = await dbContext.Set<ApplicationUser>()
                .Select(user => user.Id)
                .ToArrayAsync(TestContext.Current.CancellationToken);

            remainingUserIds.ShouldNotContain(staleUnconfirmedUser.Id);
            remainingUserIds.ShouldContain(recentUnconfirmedUser.Id);
            remainingUserIds.ShouldContain(staleConfirmedUser.Id);
        }
    }

    private static ApplicationUser CreateUser(string email)
    {
        return new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            Email = email,
            UserName = email,
            Status = AccountStatus.Active.Name,
            LockoutEnabled = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static Task SetCreatedAtAsync(
        IdentityWriteDbContext dbContext,
        ApplicationUser user,
        DateTimeOffset createdAtUtc)
    {
        return dbContext.Set<ApplicationUser>()
            .Where(storedUser => storedUser.Id == user.Id)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    storedUser => storedUser.CreatedAt,
                    createdAtUtc.UtcDateTime),
                TestContext.Current.CancellationToken);
    }

    private sealed class FrozenTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
        {
            return utcNow;
        }
    }
}
