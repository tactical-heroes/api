using DomainEvent = PANiXiDA.TacticalHeroes.Notifications.Domain.Notifications.Events.PasswordResetNotificationRequested;
using IntegrationEvent = PANiXiDA.TacticalHeroes.Identity.Contracts.Users.PasswordResetRequested;

namespace PANiXiDA.TacticalHeroes.Notifications.Infrastructure.Messaging.Handlers;

public static class PasswordResetRequestedHandler
{
    public static DomainEvent Handle(IntegrationEvent integrationEvent)
    {
        return new DomainEvent(
            IntegrationEventId: integrationEvent.Id,
            UserId: integrationEvent.UserId,
            Email: integrationEvent.Email,
            PasswordResetUrl: integrationEvent.PasswordResetUrl,
            ExpiresAtUtc: integrationEvent.ExpiresAtUtc);
    }
}
