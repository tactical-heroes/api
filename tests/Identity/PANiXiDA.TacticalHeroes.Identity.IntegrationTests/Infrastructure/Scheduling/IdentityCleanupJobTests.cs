using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Scheduling.Cleanup;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Scheduling.Options;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.Scheduling;

public sealed class IdentityCleanupJobTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    private const string Password = "StrongPassword1";

    private static readonly DateTimeOffset NowUtc = new(
        2026,
        6,
        26,
        12,
        0,
        0,
        TimeSpan.Zero);

    [Fact(DisplayName = "Prune unconfirmed users should delete only stale unconfirmed users")]
    public async Task PruneUnconfirmedUsersJob_Should_DeleteOnlyStaleUnconfirmedUsers()
    {
        var staleUnconfirmedUser = CreateUser("stale-unconfirmed@example.com");
        var recentUnconfirmedUser = CreateUser("recent-unconfirmed@example.com");
        var staleConfirmedUser = CreateUser("stale-confirmed@example.com");

        staleConfirmedUser
            .ConfirmRegistration()
            .IsSuccess
            .ShouldBeTrue();

        await using (var scope = Fixture.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
            var usersRepository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            await unitOfWork.ExecuteInTransactionAsync(
                async ct =>
                {
                    await usersRepository.AddAsync(staleUnconfirmedUser, Password, ct);
                    await usersRepository.AddAsync(recentUnconfirmedUser, Password, ct);
                    await usersRepository.AddAsync(staleConfirmedUser, Password, ct);
                },
                TestContext.Current.CancellationToken);

            await SetCreatedAtAsync(dbContext, staleUnconfirmedUser, NowUtc.AddDays(-8));
            await SetCreatedAtAsync(dbContext, recentUnconfirmedUser, NowUtc.AddDays(-1));
            await SetCreatedAtAsync(dbContext, staleConfirmedUser, NowUtc.AddDays(-8));
        }

        await using (var scope = Fixture.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
            var job = new PruneUnconfirmedUsersJob(
                dbContext,
                new FrozenTimeProvider(NowUtc),
                Options.Create(new IdentityCleanupOptions
                {
                    UnconfirmedUserRetention = TimeSpan.FromDays(7)
                }));

            await job.ExecuteAsync(TestContext.Current.CancellationToken);
        }

        await using (var scope = Fixture.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
            var remainingUserIds = await dbContext.Set<ApplicationUser>()
                .Select(user => user.Id)
                .ToArrayAsync(TestContext.Current.CancellationToken);

            remainingUserIds.ShouldNotContain(staleUnconfirmedUser.Id.Value);
            remainingUserIds.ShouldContain(recentUnconfirmedUser.Id.Value);
            remainingUserIds.ShouldContain(staleConfirmedUser.Id.Value);
        }
    }

    private static User CreateUser(string email)
    {
        return User.Register(email).Value;
    }

    private static Task SetCreatedAtAsync(
        IdentityWriteDbContext dbContext,
        User user,
        DateTimeOffset createdAtUtc)
    {
        return dbContext.Set<ApplicationUser>()
            .Where(storedUser => storedUser.Id == user.Id.Value)
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
