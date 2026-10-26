using PANiXiDA.TacticalHeroes.Matchmaking.Domain.BuildSnapshots;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets.ValueObjects;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Players;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments.Entities.MatchParticipants;

public sealed class MatchParticipant : Entity<MatchParticipantId>
{
    private MatchParticipant(
        MatchParticipantId id,
        MatchAssignmentId matchAssignmentId,
        MatchmakingTicketId ticketId,
        PlayerId playerId,
        BuildSnapshotId buildSnapshotId,
        RatingSnapshot rating)
        : base(id)
    {
        MatchAssignmentId = matchAssignmentId;
        TicketId = ticketId;
        PlayerId = playerId;
        BuildSnapshotId = buildSnapshotId;
        Rating = rating;
    }

    public MatchAssignmentId MatchAssignmentId { get; private set; }
    public MatchmakingTicketId TicketId { get; private set; }
    public PlayerId PlayerId { get; private set; }
    public BuildSnapshotId BuildSnapshotId { get; private set; }
    public RatingSnapshot Rating { get; private set; }

    internal static MatchParticipant Create(MatchAssignmentId assignmentId, MatchmakingTicket ticket)
    {
        return new MatchParticipant(
            id: MatchParticipantId.New(),
            matchAssignmentId: assignmentId,
            ticketId: ticket.Id,
            playerId: ticket.PlayerId,
            buildSnapshotId: ticket.BuildSnapshotId,
            rating: ticket.Rating);
    }
}
