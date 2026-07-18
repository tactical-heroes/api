using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Events;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Register;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.ResendConfirmationEmail;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Auth.ResendConfirmationEmail;

public sealed class ResendConfirmationEmailEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "POST auth resend-confirmation-email should issue a new token for an unconfirmed user")]
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
}
