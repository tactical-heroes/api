using DomainEvent = PANiXiDA.TacticalHeroes.Games.Domain.Games.Events.GameCreationRequested;
using IntegrationEvent = PANiXiDA.TacticalHeroes.Matchmaking.Contracts.MatchSearches.MatchFound;

namespace PANiXiDA.TacticalHeroes.Games.Infrastructure.Messaging.Handlers;

public static class MatchFoundHandler
{
    public static DomainEvent Handle(IntegrationEvent integrationEvent)
    {
        return new DomainEvent(
            FirstMatchSearchId: integrationEvent.FirstMatchSearchId,
            FirstPlayerId: integrationEvent.FirstPlayerId,
            FirstBuildId: integrationEvent.FirstBuildId,
            FirstRating: integrationEvent.FirstRating,
            SecondMatchSearchId: integrationEvent.SecondMatchSearchId,
            SecondPlayerId: integrationEvent.SecondPlayerId,
            SecondBuildId: integrationEvent.SecondBuildId,
            SecondRating: integrationEvent.SecondRating,
            Mode: integrationEvent.Mode,
            MatchedAt: integrationEvent.MatchedAt);
    }
}
