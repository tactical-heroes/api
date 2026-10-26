namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments.Events;

public sealed record GameCreationRequested(Guid MatchAssignmentId) : DomainEvent;
