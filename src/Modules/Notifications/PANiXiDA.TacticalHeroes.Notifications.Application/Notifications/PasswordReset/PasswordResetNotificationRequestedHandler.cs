using PANiXiDA.TacticalHeroes.Modules.Notifications.Application.Abstractions.Email;
using PANiXiDA.TacticalHeroes.Modules.Notifications.Domain.Notifications.Events;

namespace PANiXiDA.TacticalHeroes.Modules.Notifications.Application.Notifications.PasswordReset;

public sealed class PasswordResetNotificationRequestedHandler(
    IEmailSender emailSender) : IEventHandler<PasswordResetNotificationRequested>
{
    public Task HandleAsync(
        PasswordResetNotificationRequested domainEvent,
        CancellationToken cancellationToken)
    {
        return emailSender.SendAsync(
            EmailNotificationMessageFactory.Create(domainEvent),
            cancellationToken);
    }
}
