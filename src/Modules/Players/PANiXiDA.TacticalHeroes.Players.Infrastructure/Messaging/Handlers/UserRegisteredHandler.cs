using DomainEvent = PANiXiDA.TacticalHeroes.Modules.Players.Domain.Players.Events.PlayerRegistrationRequested;
using IntegrationEvent = PANiXiDA.TacticalHeroes.Modules.Identity.Contracts.Users.UserRegistered;

namespace PANiXiDA.TacticalHeroes.Modules.Players.Infrastructure.Messaging.Handlers;

public static class UserRegisteredHandler
{
    public static DomainEvent Handle(IntegrationEvent integrationEvent)
    {
        return new DomainEvent(UserId: integrationEvent.UserId);
    }
}
