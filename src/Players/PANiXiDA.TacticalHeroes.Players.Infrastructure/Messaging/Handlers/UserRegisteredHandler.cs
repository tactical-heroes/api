using DomainEvent = PANiXiDA.TacticalHeroes.Players.Domain.Players.Events.PlayerRegistrationRequested;
using IntegrationEvent = PANiXiDA.TacticalHeroes.Identity.Contracts.Users.UserRegistered;

namespace PANiXiDA.TacticalHeroes.Players.Infrastructure.Messaging.Handlers;

public static class UserRegisteredHandler
{
    public static DomainEvent Handle(IntegrationEvent integrationEvent)
    {
        return new DomainEvent(
            IntegrationEventId: integrationEvent.Id,
            UserId: integrationEvent.UserId);
    }
}
