using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Games;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Matches.Entities.MatchParticipants;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Matches.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Matches.Events;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.Matches;

public sealed class Match : AggregateRoot<MatchId>
{
    private readonly List<MatchParticipant> _participants = [];

    private Match(
        MatchId id,
        MatchmakingMode mode,
        MatchmakingTimestamp matchedAt,
        MatchmakingTimestamp creationDeadline)
        : base(id)
    {
        Mode = mode;
        MatchedAt = matchedAt;
        CreationDeadline = creationDeadline;
        Status = MatchStatus.CreatingGame;
    }

    public MatchmakingMode Mode { get; private set; }
    public MatchmakingTimestamp MatchedAt { get; private set; }
    public MatchmakingTimestamp CreationDeadline { get; private set; }
    public MatchStatus Status { get; private set; }
    public GameId? GameId { get; private set; }

    public IReadOnlyCollection<MatchParticipant> Participants => _participants.AsReadOnly();

    public static Result<Match> Create(
        MatchSearch first,
        MatchSearch second,
        MatchmakingTimestamp now,
        MatchmakingTimestamp creationDeadline)
    {
        if (first.Id == second.Id || first.PlayerId == second.PlayerId || first.Mode != second.Mode)
        {
            return Result.Failure<Match>(
                Error.Validation("A duel requires distinct players and searches in the same mode."));
        }

        if (creationDeadline.Value <= now.Value)
        {
            return Result.Failure<Match>(
                Error.Validation("Game creation deadline must follow match time."));
        }

        if (!IsActive(first, now) || !IsActive(second, now))
        {
            return Result.Failure<Match>(Error.Conflict("Both searches must be active."));
        }

        var match = new Match(
            id: MatchId.New(),
            mode: first.Mode,
            matchedAt: now,
            creationDeadline: creationDeadline);
        match._participants.Add(MatchParticipant.Create(match.Id, first));
        match._participants.Add(MatchParticipant.Create(match.Id, second));
        match.RecordStateChanged();
        match.AddDomainEvent(new GameCreationRequested(match.Id.Value));

        return Result.Success(match);
    }

    public Result Complete(GameId gameId, MatchmakingTimestamp now)
    {
        if (Status == MatchStatus.Ready && GameId == gameId)
        {
            return Result.Success();
        }

        if (Status != MatchStatus.CreatingGame
            || now.Value < MatchedAt.Value
            || now.Value >= CreationDeadline.Value)
        {
            return Result.Failure(Error.Conflict("Only an active game creation can complete."));
        }

        GameId = gameId;
        Status = MatchStatus.Ready;
        RecordStateChanged();

        return Result.Success();
    }

    public Result Fail()
    {
        if (Status == MatchStatus.Failed)
        {
            return Result.Success();
        }

        return Status == MatchStatus.CreatingGame
            ? Close(MatchStatus.Failed)
            : Result.Failure(Error.Conflict("Only game creation can fail."));
    }

    public Result Expire(MatchmakingTimestamp now)
    {
        if (Status == MatchStatus.Expired)
        {
            return Result.Success();
        }

        if (Status != MatchStatus.CreatingGame || now.Value < CreationDeadline.Value)
        {
            return Result.Failure(Error.Conflict("Only overdue game creation can expire."));
        }

        return Close(MatchStatus.Expired);
    }

    private static bool IsActive(MatchSearch search, MatchmakingTimestamp now)
    {
        return search.Status == MatchSearchStatus.Searching
            && now.Value >= search.QueuedAt.Value
            && now.Value < search.ExpiresAt.Value;
    }

    private Result Close(MatchStatus status)
    {
        Status = status;
        RecordStateChanged();

        return Result.Success();
    }

    private void RecordStateChanged()
    {
        AddDomainEvent(new MatchStateChanged(
            MatchId: Id.Value,
            Status: Status.Name,
            GameId: GameId?.Value));
    }
}
