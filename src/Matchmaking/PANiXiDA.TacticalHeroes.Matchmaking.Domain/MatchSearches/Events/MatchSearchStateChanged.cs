namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Events;

public sealed record MatchSearchStateChanged(
    Guid MatchSearchId,
    Guid PlayerId,
    string Status) : DomainEvent;
