using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments.Entities.MatchParticipants;

public sealed class MatchParticipant : Entity<MatchParticipantId>
{
    private MatchParticipant(
        MatchParticipantId id,
        MatchAssignmentId matchAssignmentId,
        MatchmakingTicketId ticketId,
        PlayerReference playerId,
        BuildSnapshotReference buildSnapshotId,
        RatingSnapshot rating) : base(id)
    {
        MatchAssignmentId = matchAssignmentId;
        TicketId = ticketId;
        PlayerId = playerId;
        BuildSnapshotId = buildSnapshotId;
        Rating = rating;
    }

    public MatchAssignmentId MatchAssignmentId
    {
        get; private set;
    }

    public MatchmakingTicketId TicketId
    {
        get; private set;
    }

    public PlayerReference PlayerId
    {
        get; private set;
    }

    public BuildSnapshotReference BuildSnapshotId
    {
        get; private set;
    }

    public RatingSnapshot Rating
    {
        get; private set;
    }

    public MatchmakingTimestamp? AcceptedAt
    {
        get; private set;
    }

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

    internal void Confirm(MatchmakingTimestamp now)
    {
        AcceptedAt ??= now;
    }
}
