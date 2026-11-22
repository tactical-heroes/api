namespace PANiXiDA.TacticalHeroes.Modules.Matchmaking.Domain.MatchSearches.Events;

public sealed record MatchSearchStateChanged(
    Guid MatchSearchId,
    Guid PlayerId,
    string Status,
    Guid? GameId) : DomainEvent;
