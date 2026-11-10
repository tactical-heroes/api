using DomainEvent = PANiXiDA.TacticalHeroes.Games.Domain.Games.Events.BattleAssignmentRequested;
using IntegrationEvent = PANiXiDA.TacticalHeroes.Battle.Contracts.Battles.BattleCreated;

namespace PANiXiDA.TacticalHeroes.Games.Infrastructure.Messaging.Handlers;

public static class BattleCreatedHandler
{
    public static DomainEvent Handle(IntegrationEvent integrationEvent)
    {
        return new DomainEvent(GameId: integrationEvent.GameId, BattleId: integrationEvent.BattleId);
    }
}
