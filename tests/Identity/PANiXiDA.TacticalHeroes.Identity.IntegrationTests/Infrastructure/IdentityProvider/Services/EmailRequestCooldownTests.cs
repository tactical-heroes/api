using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Events;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.EmailRequests;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Scheduling.Cleanup;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.IdentityProvider.Services;

public sealed class EmailRequestCooldownTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "Login should retain account lockout when too many passwords are incorrect")]
    public async Task LoginAsync_Should_RetainAccountLockout_When_TooManyPasswordsAreIncorrect()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "locked-user",
            Email = "locked@example.test",
            EmailConfirmed = true,
            LockoutEnabled = true
        };
        (await manager.CreateAsync(user, "StrongPassword1!")).Succeeded.ShouldBeTrue();
        var service = scope.ServiceProvider.GetRequiredService<IUserCredentialsService>();
        for (var attempt = 0; attempt < manager.Options.Lockout.MaxFailedAccessAttempts; attempt++)
        {
            (await service.LoginAsync(user.Email, "WrongPassword1!", cancellationToken)).IsFailure.ShouldBeTrue();
        }

        var result = await service.LoginAsync(user.Email, "StrongPassword1!", cancellationToken);

        result.IsFailure.ShouldBeTrue();
        (await manager.IsLockedOutAsync(user)).ShouldBeTrue();
    }

    [Fact(DisplayName = "Cooldown should allow one request when concurrent scopes target same email and purpose")]
    public async Task TryAcquireAsync_Should_AllowOneRequest_When_ConcurrentScopesTargetSameEmailAndPurpose()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var results = await Task.WhenAll(Enumerable.Range(0, 8).Select(async _ =>
        {
            await using var scope = Fixture.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            var store = scope.ServiceProvider.GetRequiredService<EmailRequestCooldownStore>();
            var acquired = await store.TryAcquireAsync(
                "CONCURRENT@EXAMPLE.TEST",
                EmailRequestPurpose.PasswordReset,
                cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return acquired;
        }));

        results.Count(acquired => acquired).ShouldBe(1);
    }

    [Fact(DisplayName = "Cooldown should allow retry when previous transaction rolls back")]
    public async Task TryAcquireAsync_Should_AllowRetry_When_PreviousTransactionRollsBack()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var store = scope.ServiceProvider.GetRequiredService<EmailRequestCooldownStore>();
        await using (var transaction = await context.Database.BeginTransactionAsync(cancellationToken))
        {
            (await store.TryAcquireAsync("ROLLBACK@EXAMPLE.TEST", EmailRequestPurpose.PasswordReset, cancellationToken))
                .ShouldBeTrue();
            await transaction.RollbackAsync(cancellationToken);
        }

        await using var retry = await context.Database.BeginTransactionAsync(cancellationToken);
        (await store.TryAcquireAsync("ROLLBACK@EXAMPLE.TEST", EmailRequestPurpose.PasswordReset, cancellationToken))
            .ShouldBeTrue();
        await retry.CommitAsync(cancellationToken);
    }

    [Fact(DisplayName = "Cooldown should isolate emails and purposes when previous request is active")]
    public async Task TryAcquireAsync_Should_IsolateEmailsAndPurposes_When_PreviousRequestIsActive()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var store = scope.ServiceProvider.GetRequiredService<EmailRequestCooldownStore>();
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        (await store.TryAcquireAsync("FIRST@EXAMPLE.TEST", EmailRequestPurpose.Confirmation, cancellationToken))
            .ShouldBeTrue();
        var cooldowns = context.Set<EmailRequestCooldown>().AsNoTracking();
        var expiresAt = (await cooldowns.SingleAsync(cancellationToken)).ExpiresAt;
        (await store.TryAcquireAsync("FIRST@EXAMPLE.TEST", EmailRequestPurpose.Confirmation, cancellationToken))
            .ShouldBeFalse();
        (await cooldowns.SingleAsync(cancellationToken)).ExpiresAt.ShouldBe(expiresAt);
        (await store.TryAcquireAsync("FIRST@EXAMPLE.TEST", EmailRequestPurpose.PasswordReset, cancellationToken))
            .ShouldBeTrue();
        (await store.TryAcquireAsync("SECOND@EXAMPLE.TEST", EmailRequestPurpose.Confirmation, cancellationToken))
            .ShouldBeTrue();
        await transaction.CommitAsync(cancellationToken);
    }

    [Fact(DisplayName = "Cooldown should allow another request when stored interval expires")]
    public async Task TryAcquireAsync_Should_AllowAnotherRequest_When_StoredIntervalExpires()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var store = scope.ServiceProvider.GetRequiredService<EmailRequestCooldownStore>();
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        (await store.TryAcquireAsync("EXPIRED@EXAMPLE.TEST", EmailRequestPurpose.PasswordReset, cancellationToken))
            .ShouldBeTrue();
        await context.Set<EmailRequestCooldown>().ExecuteUpdateAsync(
            setters => setters.SetProperty(
                cooldown => cooldown.ExpiresAt,
                DateTimeOffset.UtcNow.AddMinutes(-1)),
            cancellationToken);

        (await store.TryAcquireAsync("EXPIRED@EXAMPLE.TEST", EmailRequestPurpose.PasswordReset, cancellationToken))
            .ShouldBeTrue();
        await transaction.CommitAsync(cancellationToken);
    }

    [Fact(DisplayName = "Cleanup should retain active cooldowns when expired records are removed")]
    public async Task Execute_Should_RetainActiveCooldowns_When_ExpiredRecordsAreRemoved()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        context.AddRange(
            new EmailRequestCooldown
            {
                NormalizedEmail = "EXPIRED@EXAMPLE.TEST",
                Purpose = EmailRequestPurpose.PasswordReset,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1)
            },
            new EmailRequestCooldown
            {
                NormalizedEmail = "ACTIVE@EXAMPLE.TEST",
                Purpose = EmailRequestPurpose.PasswordReset,
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
            });
        await context.SaveChangesAsync(cancellationToken);

        await new PruneEmailRequestCooldownsJob(context).Execute(null!, cancellationToken);

        var remaining = await context.Set<EmailRequestCooldown>().AsNoTracking().SingleAsync(cancellationToken);
        remaining.NormalizedEmail.ShouldBe("ACTIVE@EXAMPLE.TEST");
    }

    [Theory(DisplayName = "Email requests should share normalization and emit one event when email case differs")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RequestEmail_Should_ShareNormalizationAndEmitOneEvent_When_EmailCaseDiffers(bool confirmed)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using (var seed = Fixture.CreateScope())
        {
            var manager = seed.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "cooldown-user",
                Email = "Cooldown@Example.Test",
                EmailConfirmed = confirmed
            };
            (await manager.CreateAsync(user, "StrongPassword1!")).Succeeded.ShouldBeTrue();
        }

        foreach (var email in new[] { "cooldown@example.test", "COOLDOWN@EXAMPLE.TEST" })
        {
            await using var scope = Fixture.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            var service = scope.ServiceProvider.GetRequiredService<IUserCredentialsService>();
            var result = confirmed
                ? await service.ForgotPasswordAsync(email, cancellationToken)
                : await service.ResendConfirmationEmailAsync(email, cancellationToken);
            result.IsSuccess.ShouldBeTrue();
            var events = scope.ServiceProvider.GetRequiredService<IAggregateTracker>().GetAll()
                .SelectMany(user => user.GetDomainEvents())
                .ToArray();
            if (email == "cooldown@example.test")
            {
                events.ShouldHaveSingleItem();
                if (confirmed)
                {
                    events[0].ShouldBeOfType<PasswordResetRequested>();
                }
                else
                {
                    events[0].ShouldBeOfType<EmailConfirmationRequested>();
                }
            }
            else
            {
                events.ShouldBeEmpty();
            }

            await transaction.CommitAsync(cancellationToken);
        }
    }
}
