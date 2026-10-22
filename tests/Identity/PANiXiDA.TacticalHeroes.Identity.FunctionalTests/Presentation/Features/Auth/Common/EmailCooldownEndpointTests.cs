using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Events;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Auth.Common;

public sealed class EmailCooldownEndpointTests(FunctionalTestFixture fixture) : FunctionalTestBase(fixture)
{
    [Theory(DisplayName = "Email endpoints should preserve success responses when account is unknown or ineligible")]
    [InlineData("forgot-password", false, false, HttpStatusCode.Accepted)]
    [InlineData("forgot-password", true, true, HttpStatusCode.Accepted)]
    [InlineData("resend-confirmation-email", true, false, HttpStatusCode.NoContent)]
    [InlineData("resend-confirmation-email", false, true, HttpStatusCode.NoContent)]
    public async Task PostEmail_Should_PreserveSuccessResponses_When_AccountIsUnknownOrIneligible(
        string endpoint, bool confirmed, bool blocked, HttpStatusCode expected)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await CreateUserAsync(confirmed, blocked);

        foreach (var email in new[] { "known@example.test", "unknown@example.test" })
        {
            using var response = await Client.PostAsJsonAsync(
                $"/api/v1/auth/{endpoint}", new { Email = email }, cancellationToken);
            response.StatusCode.ShouldBe(expected);
            response.Headers.RetryAfter.ShouldBeNull();
            (await response.Content.ReadAsStringAsync(cancellationToken)).ShouldBeEmpty();
        }

        Fixture.EventBus.Count<PasswordResetRequested>().ShouldBe(0);
        Fixture.EventBus.Count<EmailConfirmationRequested>().ShouldBe(0);
    }

    [Theory(DisplayName = "Email endpoints should suppress repeated events when email cooldown is active")]
    [InlineData("forgot-password", true, HttpStatusCode.Accepted)]
    [InlineData("resend-confirmation-email", false, HttpStatusCode.NoContent)]
    public async Task PostEmail_Should_SuppressRepeatedEvents_When_EmailCooldownIsActive(
        string endpoint, bool confirmed, HttpStatusCode expected)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await CreateUserAsync(confirmed, blocked: false);

        foreach (var email in new[] { "known@example.test", "KNOWN@EXAMPLE.TEST", "unknown@example.test" })
        {
            using var response = await Client.PostAsJsonAsync(
                $"/api/v1/auth/{endpoint}", new { Email = email }, cancellationToken);
            response.StatusCode.ShouldBe(expected);
            response.Headers.RetryAfter.ShouldBeNull();
            (await response.Content.ReadAsStringAsync(cancellationToken)).ShouldBeEmpty();
        }

        Fixture.EventBus.Count<PasswordResetRequested>().ShouldBe(confirmed ? 1 : 0);
        Fixture.EventBus.Count<EmailConfirmationRequested>().ShouldBe(confirmed ? 0 : 1);
    }

    private async Task CreateUserAsync(bool confirmed, bool blocked)
    {
        await using var scope = Fixture.Services.CreateAsyncScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "known-user",
            Email = "known@example.test",
            EmailConfirmed = confirmed,
            Status = blocked ? UserStatus.Blocked.Name : UserStatus.Active.Name
        };
        (await manager.CreateAsync(user, "StrongPassword1!")).Succeeded.ShouldBeTrue();
    }
}
