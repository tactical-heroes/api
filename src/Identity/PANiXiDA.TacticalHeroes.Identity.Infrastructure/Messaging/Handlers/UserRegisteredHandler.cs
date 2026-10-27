using DomainEvent = PANiXiDA.TacticalHeroes.Identity.Domain.Users.Events.UserRegistered;
using IntegrationEvent = PANiXiDA.TacticalHeroes.Identity.Contracts.Users.UserRegistered;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Messaging.Handlers;

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
