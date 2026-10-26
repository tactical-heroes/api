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
        MatchmakingTimestamp matchedAt,
        MatchmakingTimestamp creationDeadline)
        : base(id)
    {
        Mode = mode;
        MatchedAt = matchedAt;
        CreationDeadline = creationDeadline;
        Status = MatchAssignmentStatus.CreatingGame;
    }

    public MatchmakingMode Mode { get; private set; }
    public MatchmakingTimestamp MatchedAt { get; private set; }
    public MatchmakingTimestamp CreationDeadline { get; private set; }
    public MatchAssignmentStatus Status { get; private set; }
    public GameReference? GameId { get; private set; }
    public MatchmakingReason? ClosureReason { get; private set; }

    public IReadOnlyCollection<MatchParticipant> Participants => _participants.AsReadOnly();

    /// <summary>
    /// Captures a matched pair and requests game creation without mutating its tickets. The application must
    /// reserve both tickets and persist all three aggregates in one transaction before publishing events.
    /// </summary>
    public static Result<MatchAssignment> Create(
        MatchmakingTicket first,
        MatchmakingTicket second,
        MatchmakingTimestamp now,
        MatchmakingTimestamp creationDeadline)
    {
        if (first.Id == second.Id || first.PlayerId == second.PlayerId || first.Mode != second.Mode)
        {
            return Result.Failure<MatchAssignment>(
                Error.Validation("A duel requires distinct players and tickets in the same mode."));
        }

        if (creationDeadline.Value <= now.Value)
        {
            return Result.Failure<MatchAssignment>(
                Error.Validation("Game creation deadline must follow match time."));
        }

        if (!IsActive(first, now) || !IsActive(second, now))
        {
            return Result.Failure<MatchAssignment>(Error.Conflict("Both tickets must be actively searching."));
        }

        var assignment = new MatchAssignment(
            id: MatchAssignmentId.New(),
            mode: first.Mode,
            matchedAt: now,
            creationDeadline: creationDeadline);
        assignment._participants.Add(MatchParticipant.Create(assignment.Id, first));
        assignment._participants.Add(MatchParticipant.Create(assignment.Id, second));
        assignment.RecordStateChanged();
        assignment.AddDomainEvent(new GameCreationRequested(assignment.Id.Value));

        return Result.Success(assignment);
    }

    public Result Complete(GameReference gameId, MatchmakingTimestamp now)
    {
        if (Status == MatchAssignmentStatus.Ready && GameId == gameId)
        {
            return Result.Success();
        }

        if (Status != MatchAssignmentStatus.CreatingGame
            || now.Value < MatchedAt.Value
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

        if (Status != MatchAssignmentStatus.CreatingGame || now.Value < CreationDeadline.Value)
        {
            return Result.Failure(Error.Conflict("Only overdue game creation can expire."));
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
