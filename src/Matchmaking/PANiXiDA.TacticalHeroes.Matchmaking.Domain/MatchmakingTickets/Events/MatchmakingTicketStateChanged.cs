namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets.Events;

public sealed record MatchmakingTicketStateChanged(
    Guid TicketId,
    Guid PlayerId,
    string Status,
    Guid? MatchAssignmentId) : DomainEvent;
