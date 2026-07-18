using PANiXiDA.TacticalHeroes.Notifications.Application.Abstractions.Email;
using PANiXiDA.TacticalHeroes.Notifications.Application.Notifications.EmailConfirmation;
using PANiXiDA.TacticalHeroes.Notifications.Domain.Notifications.Events;

namespace PANiXiDA.TacticalHeroes.Notifications.UnitTests.Application.Notifications;

public sealed class EmailConfirmationNotificationRequestedHandlerTests
{
    [Fact(DisplayName = "Email confirmation notification should send a formatted email")]
    public async Task HandleAsync_Should_SendFormattedEmail()
    {
        var integrationEventId = Guid.CreateVersion7();
        var confirmationUrl =
            "https://localhost:5173/confirm-email?userId=0198f65b-b53a-7a93-940c-0d84f82e4d2a&token=confirmation-token";
        var expiresAtUtc = new DateTimeOffset(2026, 7, 19, 12, 0, 0, TimeSpan.Zero);
        var domainEvent = new EmailConfirmationNotificationRequested(
            IntegrationEventId: integrationEventId,
            UserId: Guid.CreateVersion7(),
            Email: "hero@example.com",
            ConfirmationUrl: confirmationUrl,
            ExpiresAtUtc: expiresAtUtc);
        var emailSender = Substitute.For<IEmailSender>();
        EmailMessage? sentMessage = null;
        emailSender
            .SendAsync(
                Arg.Do<EmailMessage>(message => sentMessage = message),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        var handler = new EmailConfirmationNotificationRequestedHandler(emailSender);

        await handler.HandleAsync(
            domainEvent,
            TestContext.Current.CancellationToken);

        await emailSender.Received(1).SendAsync(
            Arg.Any<EmailMessage>(),
            TestContext.Current.CancellationToken);
        sentMessage.ShouldNotBeNull();
        sentMessage.CorrelationId.ShouldBe(integrationEventId);
        sentMessage.RecipientEmail.ShouldBe("hero@example.com");
        sentMessage.Subject.ShouldBe("Confirm your Tactical Heroes email");
        sentMessage.TextBody.ShouldContain(confirmationUrl);
        sentMessage.HtmlBody.ShouldContain("Confirm your email");
        sentMessage.HtmlBody.ShouldContain("&amp;token=confirmation-token");
        sentMessage.HtmlBody.ShouldContain("2026-07-19 12:00 UTC");
    }
}
