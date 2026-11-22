namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Events;

public sealed record GameAssignmentRequested(
    Guid GameId,
    Guid FirstMatchSearchId,
    Guid SecondMatchSearchId) : DomainEvent;
