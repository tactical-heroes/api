using DomainEvent = PANiXiDA.TacticalHeroes.Modules.Identity.Domain.Users.Events.UserRegistered;
using IntegrationEvent = PANiXiDA.TacticalHeroes.Modules.Identity.Contracts.Users.UserRegistered;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Infrastructure.Messaging.Handlers;

public static class UserRegisteredHandler
{
    public static IntegrationEvent Handle(DomainEvent domainEvent)
    {
        return new IntegrationEvent(UserId: domainEvent.UserId)
        {
            Id = domainEvent.Id,
            OccurredOnUtc = domainEvent.OccurredOnUtc
        };
    }
}
