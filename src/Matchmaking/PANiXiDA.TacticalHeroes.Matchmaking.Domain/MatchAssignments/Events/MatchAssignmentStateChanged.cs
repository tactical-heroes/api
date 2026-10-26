namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments.Events;

public sealed record MatchAssignmentStateChanged(
    Guid MatchAssignmentId,
    string Status,
    Guid? GameId) : DomainEvent;
