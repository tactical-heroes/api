namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.Matches.Events;

public sealed record MatchStateChanged(
    Guid MatchId,
    string Status,
    Guid? GameId) : DomainEvent;
