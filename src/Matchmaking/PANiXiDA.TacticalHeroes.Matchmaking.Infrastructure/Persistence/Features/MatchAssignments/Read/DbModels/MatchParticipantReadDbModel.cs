using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchmakingTickets.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchAssignments.Read.DbModels;

public sealed class MatchParticipantReadDbModel : ReadDbModel<Guid>
{
    public Guid MatchAssignmentId { get; set; }
    public Guid TicketId { get; set; }
    public Guid PlayerId { get; set; }
    public Guid BuildSnapshotId { get; set; }
    public int Rating { get; set; }

    public MatchAssignmentReadDbModel? MatchAssignment { get; set; }
    public MatchmakingTicketReadDbModel? Ticket { get; set; }
}
