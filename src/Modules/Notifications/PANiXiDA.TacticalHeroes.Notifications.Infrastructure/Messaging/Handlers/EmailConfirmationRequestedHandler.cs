using DomainEvent =
    PANiXiDA.TacticalHeroes.Modules.Notifications.Domain.Notifications.Events.EmailConfirmationNotificationRequested;
using IntegrationEvent = PANiXiDA.TacticalHeroes.Modules.Identity.Contracts.Users.EmailConfirmationRequested;

namespace PANiXiDA.TacticalHeroes.Modules.Notifications.Infrastructure.Messaging.Handlers;

public static class EmailConfirmationRequestedHandler
{
    public static DomainEvent Handle(IntegrationEvent integrationEvent)
    {
        return new DomainEvent(
            IntegrationEventId: integrationEvent.Id,
            UserId: integrationEvent.UserId,
            Email: integrationEvent.Email,
            ConfirmationUrl: integrationEvent.ConfirmationUrl,
            ExpiresAtUtc: integrationEvent.ExpiresAtUtc);
    }
}
