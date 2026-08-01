using PANiXiDA.TacticalHeroes.Identity.Contracts.Users;

using System.Net;

using PANiXiDA.TacticalHeroes.Notifications.Application.Email;

namespace PANiXiDA.TacticalHeroes.Notifications.IntegrationTests.Infrastructure.Email;

[Collection(MailpitIntegrationTestCollection.Name)]
public sealed class MailKitEmailSenderTests(MailpitIntegrationTestFixture fixture)
{
    [Fact(DisplayName = "SendAsync should deliver a formatted email through Mailpit when message is valid")]
    public async Task SendAsync_Should_DeliverEmail_When_MessageIsValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var message = new EmailMessage(
            CorrelationId: Guid.CreateVersion7(),
            RecipientEmail: "direct@example.com",
            Subject: "Direct email",
            TextBody: "Direct text body",
            HtmlBody: "<strong>Direct HTML body</strong>");

        await fixture.EmailSender.SendAsync(message, cancellationToken);

        await fixture.WaitForMessageAsync(
            message.Subject,
            message.RecipientEmail,
            cancellationToken);
        var htmlBody = await fixture.WaitForBodyAsync(
            "html",
            "Direct HTML body",
            cancellationToken);
        var textBody = await fixture.WaitForBodyAsync(
            "txt",
            message.TextBody,
            cancellationToken);

        htmlBody.ShouldContain("Direct HTML body");
        textBody.ShouldContain(message.TextBody);
    }

    [Fact(DisplayName = "Email confirmation event should send a formatted email through Mailpit when event is published")]
    public async Task EmailConfirmationRequested_Should_SendFormattedEmail_When_EventIsPublished()
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

    [Fact(DisplayName = "Password reset event should send a formatted email through Mailpit when event is published")]
    public async Task PasswordResetRequested_Should_SendFormattedEmail_When_EventIsPublished()
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
