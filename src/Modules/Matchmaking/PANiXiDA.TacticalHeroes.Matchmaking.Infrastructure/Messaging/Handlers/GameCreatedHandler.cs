using DomainEvent = PANiXiDA.TacticalHeroes.Modules.Matchmaking.Domain.MatchSearches.Events.GameAssignmentRequested;
using IntegrationEvent = PANiXiDA.TacticalHeroes.Modules.Games.Contracts.Games.GameCreated;

namespace PANiXiDA.TacticalHeroes.Modules.Matchmaking.Infrastructure.Messaging.Handlers;

public static class GameCreatedHandler
{
    public static DomainEvent Handle(IntegrationEvent integrationEvent)
    {
        return new DomainEvent(
            GameId: integrationEvent.GameId,
            FirstMatchSearchId: integrationEvent.FirstMatchSearchId,
            SecondMatchSearchId: integrationEvent.SecondMatchSearchId);
    }
}
