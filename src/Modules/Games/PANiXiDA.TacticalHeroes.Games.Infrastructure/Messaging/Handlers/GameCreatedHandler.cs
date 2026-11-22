using DomainEvent = PANiXiDA.TacticalHeroes.Games.Domain.Games.Events.GameCreated;
using IntegrationEvent = PANiXiDA.TacticalHeroes.Games.Contracts.Games.GameCreated;

namespace PANiXiDA.TacticalHeroes.Games.Infrastructure.Messaging.Handlers;

public static class GameCreatedHandler
{
    public static IntegrationEvent Handle(DomainEvent domainEvent)
    {
        return new IntegrationEvent(
            GameId: domainEvent.GameId,
            FirstMatchSearchId: domainEvent.FirstMatchSearchId,
            SecondMatchSearchId: domainEvent.SecondMatchSearchId)
        {
            Id = domainEvent.Id,
            OccurredOnUtc = domainEvent.OccurredOnUtc
        };
    }
}
