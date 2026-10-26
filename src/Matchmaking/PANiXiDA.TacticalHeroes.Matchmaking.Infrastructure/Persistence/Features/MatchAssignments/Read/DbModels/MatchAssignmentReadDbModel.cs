using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchmakingTickets.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchAssignments.Read.DbModels;

public sealed class MatchAssignmentReadDbModel : AuditableReadDbModel<Guid>
{
    public string Mode { get; set; } = string.Empty;
    public DateTimeOffset ProposedAt { get; set; }
    public DateTimeOffset AcceptanceDeadline { get; set; }
    public DateTimeOffset CreationDeadline { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? GameId { get; set; }
    public string? ClosureReason { get; set; }

    public ICollection<MatchParticipantReadDbModel> Participants { get; set; } = [];
    public ICollection<MatchmakingTicketReadDbModel> MatchmakingTickets { get; set; } = [];
}
