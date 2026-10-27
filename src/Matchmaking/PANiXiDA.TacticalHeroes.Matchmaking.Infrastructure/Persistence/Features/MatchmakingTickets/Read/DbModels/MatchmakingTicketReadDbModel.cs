using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchAssignments.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchmakingTickets.Read.DbModels;

public sealed class MatchmakingTicketReadDbModel : AuditableReadDbModel<Guid>
{
    public Guid PlayerId { get; set; }
    public Guid BuildId { get; set; }
    public int Rating { get; set; }
    public string Mode { get; set; } = string.Empty;
    public DateTimeOffset QueuedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? MatchAssignmentId { get; set; }

    public MatchAssignmentReadDbModel? MatchAssignment { get; set; }
    public ICollection<MatchParticipantReadDbModel> Participants { get; set; } = [];
}
