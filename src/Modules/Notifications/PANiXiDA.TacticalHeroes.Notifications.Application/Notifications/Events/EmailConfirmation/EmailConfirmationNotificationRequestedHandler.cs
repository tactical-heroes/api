using PANiXiDA.TacticalHeroes.Notifications.Application.Abstractions.Email;
using PANiXiDA.TacticalHeroes.Notifications.Application.Notifications.Common.Factories;
using PANiXiDA.TacticalHeroes.Notifications.Domain.Notifications.Events;

namespace PANiXiDA.TacticalHeroes.Notifications.Application.Notifications.Events.EmailConfirmation;

public sealed class EmailConfirmationNotificationRequestedHandler(
    IEmailSender emailSender) : IEventHandler<EmailConfirmationNotificationRequested>
{
    public Task HandleAsync(
        EmailConfirmationNotificationRequested domainEvent,
        CancellationToken cancellationToken)
    {
        return emailSender.SendAsync(
            EmailNotificationMessageFactory.Create(domainEvent),
            cancellationToken);
    }
}
