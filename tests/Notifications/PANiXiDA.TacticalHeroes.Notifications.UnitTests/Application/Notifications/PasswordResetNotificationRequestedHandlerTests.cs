using PANiXiDA.TacticalHeroes.Notifications.Application.Abstractions.Email;
using PANiXiDA.TacticalHeroes.Notifications.Application.Notifications.PasswordReset;
using PANiXiDA.TacticalHeroes.Notifications.Domain.Notifications.Events;

namespace PANiXiDA.TacticalHeroes.Notifications.UnitTests.Application.Notifications;

public sealed class PasswordResetNotificationRequestedHandlerTests
{
    [Fact(DisplayName = "Password reset notification should send a formatted email")]
    public async Task HandleAsync_Should_SendFormattedEmail()
    {
        var integrationEventId = Guid.CreateVersion7();
        var passwordResetUrl =
            "https://localhost:5173/reset-password?userId=0198f65b-b53a-7a93-940c-0d84f82e4d2a&token=password-reset-token";
        var expiresAtUtc = new DateTimeOffset(2026, 7, 19, 12, 0, 0, TimeSpan.Zero);
        var domainEvent = new PasswordResetNotificationRequested(
            IntegrationEventId: integrationEventId,
            UserId: Guid.CreateVersion7(),
            Email: "hero@example.com",
            PasswordResetUrl: passwordResetUrl,
            ExpiresAtUtc: expiresAtUtc);
        var emailSender = Substitute.For<IEmailSender>();
        EmailMessage? sentMessage = null;
        emailSender
            .SendAsync(
                Arg.Do<EmailMessage>(message => sentMessage = message),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        var handler = new PasswordResetNotificationRequestedHandler(emailSender);

        await handler.HandleAsync(
            domainEvent,
            TestContext.Current.CancellationToken);

        await emailSender.Received(1).SendAsync(
            Arg.Any<EmailMessage>(),
            TestContext.Current.CancellationToken);
        sentMessage.ShouldNotBeNull();
        sentMessage.CorrelationId.ShouldBe(integrationEventId);
        sentMessage.RecipientEmail.ShouldBe("hero@example.com");
        sentMessage.Subject.ShouldBe("Reset your Tactical Heroes password");
        sentMessage.TextBody.ShouldContain(passwordResetUrl);
        sentMessage.HtmlBody.ShouldContain("Reset your password");
        sentMessage.HtmlBody.ShouldContain("&amp;token=password-reset-token");
        sentMessage.HtmlBody.ShouldContain("2026-07-19 12:00 UTC");
    }
}
