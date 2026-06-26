using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserConfirmationTokens;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserConfirmationTokens.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserPasswordResetTokens;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserPasswordResetTokens.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Scheduling.Cleanup;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Scheduling.Options;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.Scheduling;

public sealed class IdentityCleanupJobTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
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
        var staleUnconfirmedUser = CreateUser("stale-unconfirmed@example.com", out _);
        var recentUnconfirmedUser = CreateUser("recent-unconfirmed@example.com", out _);
        var staleConfirmedUser = CreateUser("stale-confirmed@example.com", out var confirmationTokenHash);

        staleConfirmedUser
            .ConfirmRegistration(confirmationTokenHash, NowUtc)
            .IsSuccess
            .ShouldBeTrue();

        await using (var scope = Fixture.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();

            dbContext.Set<User>().AddRange(
                staleUnconfirmedUser,
                recentUnconfirmedUser,
                staleConfirmedUser);

            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

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
            var remainingUserIds = await dbContext.Set<User>()
                .IgnoreAutoIncludes()
                .Select(user => user.Id)
                .ToArrayAsync(TestContext.Current.CancellationToken);

            remainingUserIds.ShouldNotContain(staleUnconfirmedUser.Id);
            remainingUserIds.ShouldContain(recentUnconfirmedUser.Id);
            remainingUserIds.ShouldContain(staleConfirmedUser.Id);
        }
    }

    [Fact(DisplayName = "Prune expired confirmation tokens should delete only expired confirmation tokens")]
    public async Task PruneExpiredUserConfirmationTokensJob_Should_DeleteOnlyExpiredConfirmationTokens()
    {
        var expiredTokenUser = CreateUser("expired-confirmation-token@example.com", out _);
        var activeTokenUser = CreateUser("active-confirmation-token@example.com", out _);

        await using (var scope = Fixture.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();

            dbContext.Set<User>().AddRange(expiredTokenUser, activeTokenUser);
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            SetConfirmationTokenExpiration(dbContext, expiredTokenUser, NowUtc.AddMinutes(-1));
            SetConfirmationTokenExpiration(dbContext, activeTokenUser, NowUtc.AddMinutes(1));

            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using (var scope = Fixture.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
            var job = new PruneExpiredUserConfirmationTokensJob(
                dbContext,
                new FrozenTimeProvider(NowUtc));

            await job.ExecuteAsync(TestContext.Current.CancellationToken);
        }

        await using (var scope = Fixture.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
            var users = await dbContext.Set<User>()
                .IgnoreAutoIncludes()
                .Include(user => user.ConfirmationToken)
                .ToArrayAsync(TestContext.Current.CancellationToken);

            var remainingTokenUserIds = users
                .Where(user => user.ConfirmationToken is not null)
                .Select(user => user.Id)
                .ToArray();

            remainingTokenUserIds.ShouldNotContain(expiredTokenUser.Id);
            remainingTokenUserIds.ShouldContain(activeTokenUser.Id);
        }
    }

    [Fact(DisplayName = "Prune expired password reset tokens should delete only expired password reset tokens")]
    public async Task PruneExpiredUserPasswordResetTokensJob_Should_DeleteOnlyExpiredPasswordResetTokens()
    {
        var expiredTokenUser = CreateUserWithPasswordResetToken("expired-password-reset-token@example.com");
        var activeTokenUser = CreateUserWithPasswordResetToken("active-password-reset-token@example.com");

        await using (var scope = Fixture.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();

            dbContext.Set<User>().AddRange(expiredTokenUser, activeTokenUser);
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            SetPasswordResetTokenExpiration(dbContext, expiredTokenUser, NowUtc.AddMinutes(-1));
            SetPasswordResetTokenExpiration(dbContext, activeTokenUser, NowUtc.AddMinutes(1));

            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using (var scope = Fixture.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
            var job = new PruneExpiredUserPasswordResetTokensJob(
                dbContext,
                new FrozenTimeProvider(NowUtc));

            await job.ExecuteAsync(TestContext.Current.CancellationToken);
        }

        await using (var scope = Fixture.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
            var users = await dbContext.Set<User>()
                .IgnoreAutoIncludes()
                .Include(user => user.PasswordResetToken)
                .ToArrayAsync(TestContext.Current.CancellationToken);

            var remainingTokenUserIds = users
                .Where(user => user.PasswordResetToken is not null)
                .Select(user => user.Id)
                .ToArray();

            remainingTokenUserIds.ShouldNotContain(expiredTokenUser.Id);
            remainingTokenUserIds.ShouldContain(activeTokenUser.Id);
        }
    }

    private static User CreateUser(
        string email,
        out string confirmationTokenHash)
    {
        confirmationTokenHash = $"{email}-confirmation-token-hash";

        return User.Register(
                email,
                "password-hash",
                confirmationTokenHash,
                DateTimeOffset.UtcNow.AddHours(1),
                $"{email}-confirmation-token")
            .Value;
    }

    private static User CreateUserWithPasswordResetToken(string email)
    {
        var user = CreateUser(email, out var confirmationTokenHash);

        user.ConfirmRegistration(confirmationTokenHash, DateTimeOffset.UtcNow)
            .IsSuccess
            .ShouldBeTrue();

        user.RequestPasswordReset(
                $"{email}-password-reset-token-hash",
                DateTimeOffset.UtcNow.AddHours(1),
                $"{email}-password-reset-token")
            .IsSuccess
            .ShouldBeTrue();

        return user;
    }

    private static Task SetCreatedAtAsync(
        IdentityWriteDbContext dbContext,
        User user,
        DateTimeOffset createdAtUtc)
    {
        return dbContext.Set<User>()
            .IgnoreAutoIncludes()
            .Where(storedUser => storedUser.Id == user.Id)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    storedUser => EF.Property<DateTime>(storedUser, EfConstants.CreatedAt),
                    createdAtUtc.UtcDateTime),
                TestContext.Current.CancellationToken);
    }

    private static void SetConfirmationTokenExpiration(
        IdentityWriteDbContext dbContext,
        User user,
        DateTimeOffset expiresAtUtc)
    {
        var token = user.ConfirmationToken.ShouldNotBeNull();
        var expiresAtProperty = dbContext.Entry(token)
            .Property<ConfirmationTokenExpiration>(nameof(UserConfirmationToken.ExpiresAtUtc));

        expiresAtProperty.CurrentValue = ConfirmationTokenExpiration.Create(expiresAtUtc).Value;
        expiresAtProperty.IsModified = true;
    }

    private static void SetPasswordResetTokenExpiration(
        IdentityWriteDbContext dbContext,
        User user,
        DateTimeOffset expiresAtUtc)
    {
        var token = user.PasswordResetToken.ShouldNotBeNull();
        var expiresAtProperty = dbContext.Entry(token)
            .Property<PasswordResetTokenExpiration>(nameof(UserPasswordResetToken.ExpiresAtUtc));

        expiresAtProperty.CurrentValue = PasswordResetTokenExpiration.Create(expiresAtUtc).Value;
        expiresAtProperty.IsModified = true;
    }

    private sealed class FrozenTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
        {
            return utcNow;
        }
    }
}
