using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments.Entities.MatchParticipants;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments.Events;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments.ValueObjects;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments;

public sealed class MatchAssignment : AggregateRoot<MatchAssignmentId>
{
    private readonly List<MatchParticipant> _participants = [];

    private MatchAssignment(
        MatchAssignmentId id,
        MatchmakingMode mode,
        MatchmakingTimestamp proposedAt,
        MatchmakingTimestamp acceptanceDeadline,
        MatchmakingTimestamp creationDeadline) : base(id)
    {
        Mode = mode;
        ProposedAt = proposedAt;
        AcceptanceDeadline = acceptanceDeadline;
        CreationDeadline = creationDeadline;
        Status = MatchAssignmentStatus.AwaitingAcceptance;
    }

    public MatchmakingMode Mode
    {
        get; private set;
    }

    public MatchmakingTimestamp ProposedAt
    {
        get; private set;
    }

    public MatchmakingTimestamp AcceptanceDeadline
    {
        get; private set;
    }

    public MatchmakingTimestamp CreationDeadline
    {
        get; private set;
    }

    public MatchAssignmentStatus Status
    {
        get; private set;
    }

    public GameReference? GameId
    {
        get; private set;
    }

    public MatchmakingReason? ClosureReason
    {
        get; private set;
    }

    public IReadOnlyCollection<MatchParticipant> Participants => _participants.AsReadOnly();

    /// <summary>
    /// Captures a duel proposal without mutating its tickets. The application must reserve both tickets
    /// and persist all three aggregates in one transaction before publishing the proposal.
    /// </summary>
    public static Result<MatchAssignment> Create(
        MatchmakingTicket first,
        MatchmakingTicket second,
        MatchmakingTimestamp now,
        MatchmakingTimestamp acceptanceDeadline,
        MatchmakingTimestamp creationDeadline)
    {
        if (first.Id == second.Id || first.PlayerId == second.PlayerId || first.Mode != second.Mode)
        {
            return Result.Failure<MatchAssignment>(
                Error.Validation("A duel requires distinct players and tickets in the same mode."));
        }

        if (acceptanceDeadline.Value <= now.Value || creationDeadline.Value <= acceptanceDeadline.Value)
        {
            return Result.Failure<MatchAssignment>(
                Error.Validation("Acceptance and creation deadlines must follow proposal time in order."));
        }

        if (!IsActive(first, now) || !IsActive(second, now))
        {
            return Result.Failure<MatchAssignment>(Error.Conflict("Both tickets must be actively searching."));
        }

        var assignment = new MatchAssignment(
            id: MatchAssignmentId.New(),
            mode: first.Mode,
            proposedAt: now,
            acceptanceDeadline: acceptanceDeadline,
            creationDeadline: creationDeadline);
        assignment._participants.Add(MatchParticipant.Create(assignment.Id, first));
        assignment._participants.Add(MatchParticipant.Create(assignment.Id, second));
        assignment.RecordStateChanged();
        return Result.Success(assignment);
    }

    public Result Accept(MatchParticipantId participantId, MatchmakingTimestamp now)
    {
        var participant = _participants.SingleOrDefault(value => value.Id == participantId);
        if (participant is null)
        {
            return Result.Failure(Error.Validation("Participant does not belong to this assignment."));
        }

        if (participant.AcceptedAt is not null
            && (Status == MatchAssignmentStatus.AwaitingAcceptance
                || Status == MatchAssignmentStatus.CreatingGame
                || Status == MatchAssignmentStatus.Ready))
        {
            return Result.Success();
        }

        if (Status != MatchAssignmentStatus.AwaitingAcceptance
            || now.Value < ProposedAt.Value
            || now.Value >= AcceptanceDeadline.Value)
        {
            return Result.Failure(Error.Conflict("Assignment is not accepting confirmations."));
        }

        participant.Confirm(now);
        AddDomainEvent(new MatchParticipantAccepted(
            MatchAssignmentId: Id.Value,
            ParticipantId: participant.Id.Value,
            PlayerId: participant.PlayerId.Value,
            AcceptedAt: now.Value));
        if (_participants.All(value => value.AcceptedAt is not null))
        {
            Status = MatchAssignmentStatus.CreatingGame;
            RecordStateChanged();
            AddDomainEvent(new GameCreationRequested(Id.Value));
        }

        return Result.Success();
    }

    public Result Decline(MatchParticipantId participantId, MatchmakingTimestamp now)
    {
        if (!_participants.Any(value => value.Id == participantId))
        {
            return Result.Failure(Error.Validation("Participant does not belong to this assignment."));
        }

        if (now.Value < ProposedAt.Value || now.Value >= AcceptanceDeadline.Value)
        {
            return Result.Failure(Error.Conflict("The acceptance window is closed."));
        }

        return Cancel(MatchmakingReason.Create("Participant declined.").Value);
    }

    public Result Cancel(MatchmakingReason reason)
    {
        if (Status == MatchAssignmentStatus.Cancelled)
        {
            return Result.Success();
        }

        if (Status != MatchAssignmentStatus.AwaitingAcceptance)
        {
            return Result.Failure(Error.Conflict("Only an unconfirmed assignment can be cancelled."));
        }

        return Close(MatchAssignmentStatus.Cancelled, reason);
    }

    public Result Complete(GameReference gameId, MatchmakingTimestamp now)
    {
        if (Status == MatchAssignmentStatus.Ready && GameId == gameId)
        {
            return Result.Success();
        }

        if (Status != MatchAssignmentStatus.CreatingGame
            || now.Value < ProposedAt.Value
            || now.Value >= CreationDeadline.Value)
        {
            return Result.Failure(Error.Conflict("Only an active game creation can complete."));
        }

        GameId = gameId;
        Status = MatchAssignmentStatus.Ready;
        RecordStateChanged();
        return Result.Success();
    }

    public Result Fail(MatchmakingReason reason)
    {
        if (Status == MatchAssignmentStatus.Failed)
        {
            return Result.Success();
        }

        return Status == MatchAssignmentStatus.CreatingGame
            ? Close(MatchAssignmentStatus.Failed, reason)
            : Result.Failure(Error.Conflict("Only game creation can fail."));
    }

    public Result Expire(MatchmakingTimestamp now)
    {
        if (Status == MatchAssignmentStatus.Expired)
        {
            return Result.Success();
        }

        var deadline = Status == MatchAssignmentStatus.AwaitingAcceptance ? AcceptanceDeadline : CreationDeadline;
        if ((Status != MatchAssignmentStatus.AwaitingAcceptance && Status != MatchAssignmentStatus.CreatingGame)
            || now.Value < deadline.Value)
        {
            return Result.Failure(Error.Conflict("Only an overdue active assignment can expire."));
        }

        return Close(MatchAssignmentStatus.Expired, MatchmakingReason.Create("Assignment deadline elapsed.").Value);
    }

    private static bool IsActive(MatchmakingTicket ticket, MatchmakingTimestamp now)
    {
        return ticket.Status == MatchmakingTicketStatus.Searching
            && now.Value >= ticket.QueuedAt.Value
            && now.Value < ticket.ExpiresAt.Value;
    }

    private Result Close(MatchAssignmentStatus status, MatchmakingReason reason)
    {
        Status = status;
        ClosureReason = reason;
        RecordStateChanged();
        return Result.Success();
    }

    private void RecordStateChanged()
    {
        AddDomainEvent(new MatchAssignmentStateChanged(
            MatchAssignmentId: Id.Value,
            Status: Status.Name,
            GameId: GameId?.Value,
            Reason: ClosureReason?.Value));
    }
}
