using DomainEvent = PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Events.BattleCreated;
using IntegrationEvent = PANiXiDA.TacticalHeroes.Battle.Contracts.Battles.BattleCreated;

namespace PANiXiDA.TacticalHeroes.Battle.Infrastructure.Messaging.Handlers;

public static class BattleCreatedHandler
{
    public static IntegrationEvent Handle(DomainEvent domainEvent)
    {
        return new IntegrationEvent(GameId: domainEvent.GameId, BattleId: domainEvent.BattleId)
        {
            Id = domainEvent.Id,
            OccurredOnUtc = domainEvent.OccurredOnUtc
        };
    }
}
