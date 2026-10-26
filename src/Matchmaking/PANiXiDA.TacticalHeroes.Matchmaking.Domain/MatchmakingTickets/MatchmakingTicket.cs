using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets.Events;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets;

public sealed class MatchmakingTicket : AggregateRoot<MatchmakingTicketId>
{
    private MatchmakingTicket(
        MatchmakingTicketId id,
        PlayerReference playerId,
        BuildSnapshotReference buildSnapshotId,
        RatingSnapshot rating,
        MatchmakingMode mode,
        MatchmakingTimestamp queuedAt,
        MatchmakingTimestamp expiresAt)
        : base(id)
    {
        PlayerId = playerId;
        BuildSnapshotId = buildSnapshotId;
        Rating = rating;
        Mode = mode;
        QueuedAt = queuedAt;
        ExpiresAt = expiresAt;
        Status = MatchmakingTicketStatus.Searching;
    }

    public PlayerReference PlayerId { get; private set; }
    public BuildSnapshotReference BuildSnapshotId { get; private set; }
    public RatingSnapshot Rating { get; private set; }
    public MatchmakingMode Mode { get; private set; }
    public MatchmakingTimestamp QueuedAt { get; private set; }
    public MatchmakingTimestamp ExpiresAt { get; private set; }
    public MatchmakingTicketStatus Status { get; private set; }
    public MatchAssignmentId? MatchAssignmentId { get; private set; }

    public static Result<MatchmakingTicket> Create(
        PlayerReference playerId,
        BuildSnapshotReference buildSnapshotId,
        RatingSnapshot rating,
        MatchmakingMode mode,
        MatchmakingTimestamp queuedAt,
        MatchmakingTimestamp expiresAt)
    {
        if (expiresAt.Value <= queuedAt.Value)
        {
            return Result.Failure<MatchmakingTicket>(Error.Validation("Search expiration must follow queue entry."));
        }

        var ticket = new MatchmakingTicket(
            id: MatchmakingTicketId.New(),
            playerId: playerId,
            buildSnapshotId: buildSnapshotId,
            rating: rating,
            mode: mode,
            queuedAt: queuedAt,
            expiresAt: expiresAt);
        ticket.RecordStateChanged();

        return Result.Success(ticket);
    }

    public Result Reserve(MatchAssignmentId assignmentId, MatchmakingTimestamp now)
    {
        if (assignmentId.Value == Guid.Empty)
        {
            return Result.Failure(Error.Validation("Assignment id cannot be empty."));
        }

        if (Status == MatchmakingTicketStatus.Reserved && MatchAssignmentId == assignmentId)
        {
            return Result.Success();
        }

        if (Status != MatchmakingTicketStatus.Searching || now.Value < QueuedAt.Value || now.Value >= ExpiresAt.Value)
        {
            return Result.Failure(Error.Conflict("Only an active searching ticket can be reserved."));
        }

        MatchAssignmentId = assignmentId;
        Status = MatchmakingTicketStatus.Reserved;
        RecordStateChanged();

        return Result.Success();
    }

    public Result MarkAssigned(MatchAssignmentId assignmentId)
    {
        if (MatchAssignmentId != assignmentId || assignmentId.Value == Guid.Empty)
        {
            return Result.Failure(Error.Conflict("Ticket does not belong to this assignment."));
        }

        if (Status == MatchmakingTicketStatus.Assigned)
        {
            return Result.Success();
        }

        if (Status != MatchmakingTicketStatus.Reserved)
        {
            return Result.Failure(Error.Conflict("Only a reserved ticket can be assigned to a game."));
        }

        Status = MatchmakingTicketStatus.Assigned;
        RecordStateChanged();

        return Result.Success();
    }

    /// <summary>
    /// Closes this search. A reserved ticket may be closed only as part of recovery from failed or expired
    /// game creation, together with its assignment and peer ticket in one transaction. A retry uses a new ticket.
    /// </summary>
    public Result Cancel()
    {
        if (Status == MatchmakingTicketStatus.Cancelled)
        {
            return Result.Success();
        }

        if (Status != MatchmakingTicketStatus.Searching && Status != MatchmakingTicketStatus.Reserved)
        {
            return Result.Failure(Error.Conflict("Only a searching or reserved ticket can be cancelled."));
        }

        Status = MatchmakingTicketStatus.Cancelled;
        RecordStateChanged();

        return Result.Success();
    }

    public Result Expire(MatchmakingTimestamp now)
    {
        if (Status == MatchmakingTicketStatus.Expired)
        {
            return Result.Success();
        }

        if (Status != MatchmakingTicketStatus.Searching || now.Value < ExpiresAt.Value)
        {
            return Result.Failure(Error.Conflict("Only an overdue searching ticket can expire."));
        }

        Status = MatchmakingTicketStatus.Expired;
        RecordStateChanged();

        return Result.Success();
    }

    private void RecordStateChanged()
    {
        AddDomainEvent(new MatchmakingTicketStateChanged(
            TicketId: Id.Value,
            PlayerId: PlayerId.Value,
            Status: Status.Name,
            MatchAssignmentId: MatchAssignmentId?.Value));
    }
}
