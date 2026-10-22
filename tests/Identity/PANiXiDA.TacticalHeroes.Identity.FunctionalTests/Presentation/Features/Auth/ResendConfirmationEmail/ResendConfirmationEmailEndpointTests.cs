using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Events;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.EmailRequests;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Register;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.ResendConfirmationEmail;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Auth.ResendConfirmationEmail;

public sealed class ResendConfirmationEmailEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(
        DisplayName = "POST auth resend-confirmation-email should issue a new token for an unconfirmed user when user is unconfirmed")]
    public async Task PostResendConfirmationEmail_Should_PublishConfirmationRequest_When_UserIsUnconfirmed()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var registerResponse = await Client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterUserRequest(
                "resend@example.test",
                "resend-hero",
                "StrongPassword1!"),
            JsonOptions,
            cancellationToken);

        registerResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        Fixture.EventBus.Clear();

        await using (var scope = Fixture.Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
            await context.Set<EmailRequestCooldown>().ExecuteUpdateAsync(
                setters => setters.SetProperty(cooldown => cooldown.ExpiresAt, DateTimeOffset.UtcNow.AddMinutes(-1)),
                cancellationToken);
        }

        using var response = await Client.PostAsJsonAsync(
            "/api/v1/auth/resend-confirmation-email",
            new ResendConfirmationEmailRequest("resend@example.test"),
            JsonOptions,
            cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var confirmation = Fixture.EventBus.Single<EmailConfirmationRequested>();
        confirmation.Email.ShouldBe("resend@example.test");
        confirmation.ConfirmationToken.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact(DisplayName = "Resend confirmation should suppress a duplicate when registration has just sent confirmation")]
    public async Task PostResendConfirmationEmail_Should_SuppressDuplicate_When_RegistrationHasJustSentConfirmation()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var registration = await Client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterUserRequest("initial@example.test", "initial-hero", "StrongPassword1!"),
            JsonOptions,
            cancellationToken);
        registration.StatusCode.ShouldBe(HttpStatusCode.Created);
        Fixture.EventBus.Clear();

        using var response = await Client.PostAsJsonAsync(
            "/api/v1/auth/resend-confirmation-email",
            new ResendConfirmationEmailRequest("INITIAL@example.test"),
            JsonOptions,
            cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        response.Headers.RetryAfter.ShouldBeNull();
        Fixture.EventBus.Count<EmailConfirmationRequested>().ShouldBe(0);
    }
}
