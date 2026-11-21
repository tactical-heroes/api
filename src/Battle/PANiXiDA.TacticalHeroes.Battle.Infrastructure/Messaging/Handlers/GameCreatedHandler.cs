using DomainEvent = PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Events.BattleCreationRequested;
using IntegrationEvent = PANiXiDA.TacticalHeroes.Games.Contracts.Games.GameCreated;

namespace PANiXiDA.TacticalHeroes.Battle.Infrastructure.Messaging.Handlers;

public static class GameCreatedHandler
{
    public static DomainEvent Handle(IntegrationEvent integrationEvent)
    {
        return new DomainEvent(
            GameId: integrationEvent.GameId,
            FirstPlayerId: integrationEvent.FirstPlayerId,
            FirstBuildId: integrationEvent.FirstBuildId,
            SecondPlayerId: integrationEvent.SecondPlayerId,
            SecondBuildId: integrationEvent.SecondBuildId);
    }
}
