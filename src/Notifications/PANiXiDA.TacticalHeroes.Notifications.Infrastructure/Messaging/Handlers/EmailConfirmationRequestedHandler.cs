using DomainEvent = PANiXiDA.TacticalHeroes.Notifications.Domain.Notifications.Events.EmailConfirmationNotificationRequested;
using IntegrationEvent = PANiXiDA.TacticalHeroes.Identity.Contracts.Users.EmailConfirmationRequested;

namespace PANiXiDA.TacticalHeroes.Notifications.Infrastructure.Messaging.Handlers;

public sealed class EmailConfirmationRequestedHandler
{
    public DomainEvent Handle(IntegrationEvent integrationEvent)
    {
        return new DomainEvent(
            IntegrationEventId: integrationEvent.Id,
            UserId: integrationEvent.UserId,
            Email: integrationEvent.Email,
            ConfirmationUrl: integrationEvent.ConfirmationUrl,
            ExpiresAtUtc: integrationEvent.ExpiresAtUtc);
    }
}
