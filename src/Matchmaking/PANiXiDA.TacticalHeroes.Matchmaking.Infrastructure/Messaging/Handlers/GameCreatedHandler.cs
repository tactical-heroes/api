using DomainEvent = PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Events.GameAssignmentRequested;
using IntegrationEvent = PANiXiDA.TacticalHeroes.Games.Contracts.Games.GameCreated;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Messaging.Handlers;

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
