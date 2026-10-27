using DomainEvent = PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Events.MatchFound;
using IntegrationEvent = PANiXiDA.TacticalHeroes.Matchmaking.Contracts.MatchSearches.MatchFound;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Messaging.Handlers;

public static class MatchFoundHandler
{
    public static IntegrationEvent Handle(DomainEvent domainEvent)
    {
        return new IntegrationEvent(
            Id: domainEvent.Id,
            OccurredOnUtc: domainEvent.OccurredOnUtc,
            FirstMatchSearchId: domainEvent.FirstMatchSearchId,
            FirstPlayerId: domainEvent.FirstPlayerId,
            FirstBuildId: domainEvent.FirstBuildId,
            FirstRating: domainEvent.FirstRating,
            SecondMatchSearchId: domainEvent.SecondMatchSearchId,
            SecondPlayerId: domainEvent.SecondPlayerId,
            SecondBuildId: domainEvent.SecondBuildId,
            SecondRating: domainEvent.SecondRating,
            Mode: domainEvent.Mode,
            MatchedAt: domainEvent.MatchedAt);
    }
}
