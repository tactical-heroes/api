using PANiXiDA.TacticalHeroes.Modules.Notifications.Application.Abstractions.Email;
using PANiXiDA.TacticalHeroes.Modules.Notifications.Domain.Notifications.Events;

namespace PANiXiDA.TacticalHeroes.Modules.Notifications.Application.Notifications.EmailConfirmation;

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
