using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Events;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.EmailRequests;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Auth.Common;

public sealed class EmailCooldownOutboxTests(FunctionalTestFixture fixture) : FunctionalTestBase(fixture)
{
    [Theory(DisplayName = "Email cooldown and durable outbox should roll back together when saving command fails")]
    [InlineData("forgot-password", true, HttpStatusCode.Accepted)]
    [InlineData("resend-confirmation-email", false, HttpStatusCode.NoContent)]
    public async Task PostEmail_Should_RollBackCooldownAndOutbox_When_SavingCommandFails(
        string endpoint, bool confirmed, HttpStatusCode expected)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using (var scope = Fixture.Services.CreateAsyncScope())
        {
            var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "rollback-user",
                Email = "rollback@example.test",
                EmailConfirmed = confirmed
            };
            (await manager.CreateAsync(user, "StrongPassword1!")).Succeeded.ShouldBeTrue();
        }

        var interceptor = new FailAfterOutboxSaveInterceptor();
        await using var baseFactory = new FunctionalTestWebApplicationFactory(captureEvents: false);
        await using var factory = baseFactory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            services.AddSingleton<IInterceptor>(interceptor)));
        using var client = factory.CreateClient();
        using var failed = await client.PostAsJsonAsync(
            $"/api/v1/auth/{endpoint}", new { Email = "rollback@example.test" }, cancellationToken);

        failed.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        interceptor.CooldownWrittenInTransaction.ShouldBeTrue();
        interceptor.EnvelopesWrittenInTransaction.ShouldBeGreaterThan(0);
        await using (var scope = Fixture.Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
            (await context.Set<EmailRequestCooldown>().CountAsync(cancellationToken)).ShouldBe(0);
            (await CountEnvelopesAsync(context, cancellationToken)).ShouldBe(0);
        }

        using var retry = await Client.PostAsJsonAsync(
            $"/api/v1/auth/{endpoint}", new { Email = "rollback@example.test" }, cancellationToken);
        retry.StatusCode.ShouldBe(expected);
        Fixture.EventBus.Count<PasswordResetRequested>().ShouldBe(confirmed ? 1 : 0);
        Fixture.EventBus.Count<EmailConfirmationRequested>().ShouldBe(confirmed ? 0 : 1);
    }

    private static Task<int> CountEnvelopesAsync(IdentityWriteDbContext context, CancellationToken cancellationToken)
    {
        return context.Database.SqlQueryRaw<int>("""
            SELECT ((SELECT count(*) FROM wolverine.wolverine_incoming_envelopes)
                + (SELECT count(*) FROM wolverine.wolverine_outgoing_envelopes))::integer AS "Value"
            """)
            .SingleAsync(cancellationToken);
    }

    private sealed class FailAfterOutboxSaveInterceptor : SaveChangesInterceptor
    {
        public bool CooldownWrittenInTransaction { get; private set; }
        public int EnvelopesWrittenInTransaction { get; private set; }

        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context is IdentityWriteDbContext context && context.Database.CurrentTransaction is not null)
            {
                CooldownWrittenInTransaction = await context.Set<EmailRequestCooldown>().AnyAsync(cancellationToken);
                EnvelopesWrittenInTransaction = await CountEnvelopesAsync(context, cancellationToken);
                throw new InvalidOperationException("Simulated failure before committing the email command.");
            }

            return result;
        }
    }
}
