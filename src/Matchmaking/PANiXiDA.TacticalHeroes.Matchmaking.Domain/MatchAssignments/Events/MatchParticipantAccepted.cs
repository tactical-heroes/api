namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments.Events;

public sealed record MatchParticipantAccepted(
    Guid MatchAssignmentId,
    Guid ParticipantId,
    Guid PlayerId,
    DateTimeOffset AcceptedAt) : DomainEvent;
