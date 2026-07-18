using PANiXiDA.TacticalHeroes.Identity.Contracts.Users;

using System.Net;

namespace PANiXiDA.TacticalHeroes.Notifications.IntegrationTests;

[Collection(MailpitIntegrationTestCollection.Name)]
public sealed class NotificationEmailTests(MailpitIntegrationTestFixture fixture)
{
    [Fact(DisplayName = "Email confirmation event should send a formatted email through Mailpit")]
    public async Task EmailConfirmationRequested_Should_SendFormattedEmail()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var confirmationUrl =
            "https://localhost:5173/confirm-email?userId=0198f65b-b53a-7a93-940c-0d84f82e4d2a&token=confirmation-token";
        var integrationEvent = new EmailConfirmationRequested(
            UserId: Guid.CreateVersion7(),
            Email: "confirmation@example.com",
            ConfirmationUrl: confirmationUrl,
            ExpiresAtUtc: new DateTimeOffset(2026, 7, 19, 12, 0, 0, TimeSpan.Zero));

        await fixture.MessageBus.PublishAsync(integrationEvent);

        await fixture.WaitForMessageAsync(
            "Confirm your Tactical Heroes email",
            "confirmation@example.com",
            cancellationToken);
        var htmlBody = await fixture.WaitForBodyAsync(
            "html",
            "Confirm your email",
            cancellationToken);
        var textBody = await fixture.WaitForBodyAsync(
            "txt",
            confirmationUrl,
            cancellationToken);

        htmlBody.ShouldContain("Confirm your email");
        htmlBody.ShouldContain($"href=\"{WebUtility.HtmlEncode(confirmationUrl)}\"");
        htmlBody.ShouldContain("2026-07-19 12:00 UTC");
        textBody.ShouldContain(confirmationUrl);
        textBody.ShouldContain("2026-07-19 12:00 UTC");
    }

    [Fact(DisplayName = "Password reset event should send a formatted email through Mailpit")]
    public async Task PasswordResetRequested_Should_SendFormattedEmail()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var passwordResetUrl =
            "https://localhost:5173/reset-password?userId=0198f65b-b53a-7a93-940c-0d84f82e4d2a&token=password-reset-token";
        var integrationEvent = new PasswordResetRequested(
            UserId: Guid.CreateVersion7(),
            Email: "password-reset@example.com",
            PasswordResetUrl: passwordResetUrl,
            ExpiresAtUtc: new DateTimeOffset(2026, 7, 19, 13, 0, 0, TimeSpan.Zero));

        await fixture.MessageBus.PublishAsync(integrationEvent);

        await fixture.WaitForMessageAsync(
            "Reset your Tactical Heroes password",
            "password-reset@example.com",
            cancellationToken);
        var htmlBody = await fixture.WaitForBodyAsync(
            "html",
            "Reset your password",
            cancellationToken);
        var textBody = await fixture.WaitForBodyAsync(
            "txt",
            passwordResetUrl,
            cancellationToken);

        htmlBody.ShouldContain("Reset your password");
        htmlBody.ShouldContain($"href=\"{WebUtility.HtmlEncode(passwordResetUrl)}\"");
        htmlBody.ShouldContain("2026-07-19 13:00 UTC");
        textBody.ShouldContain(passwordResetUrl);
        textBody.ShouldContain("2026-07-19 13:00 UTC");
    }
}
