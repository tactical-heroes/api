using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Builds;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Games;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Events;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.ValueObjects;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Players;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches;

public sealed class MatchSearch : AggregateRoot<MatchSearchId>
{
    private static readonly TimeSpan s_searchLifetime = TimeSpan.FromMinutes(minutes: 5);

    private MatchSearch(
        MatchSearchId id,
        PlayerId playerId,
        BuildId buildId,
        Rating rating,
        MatchmakingMode mode,
        MatchmakingTimestamp queuedAt,
        MatchmakingTimestamp expiresAt)
        : base(id)
    {
        PlayerId = playerId;
        BuildId = buildId;
        Rating = rating;
        Mode = mode;
        QueuedAt = queuedAt;
        ExpiresAt = expiresAt;
        Status = MatchSearchStatus.Searching;
    }

    public PlayerId PlayerId { get; private set; }
    public BuildId BuildId { get; private set; }
    public GameId? GameId { get; private set; }
    public Rating Rating { get; private set; }
    public MatchmakingMode Mode { get; private set; }
    public MatchmakingTimestamp QueuedAt { get; private set; }
    public MatchmakingTimestamp ExpiresAt { get; private set; }
    public MatchSearchStatus Status { get; private set; }

    public static Result<MatchSearch> Create(
        PlayerId playerId,
        BuildId buildId,
        Rating rating,
        MatchmakingMode mode,
        MatchmakingTimestamp queuedAt)
    {
        if (queuedAt.Value > DateTimeOffset.MaxValue.Subtract(s_searchLifetime))
        {
            return Result.Failure<MatchSearch>(
                Error.Validation("Queue entry is too late to calculate search expiration."));
        }

        var expiresAt = MatchmakingTimestamp.Create(queuedAt.Value.Add(s_searchLifetime));

        var search = new MatchSearch(
            id: MatchSearchId.New(),
            playerId: playerId,
            buildId: buildId,
            rating: rating,
            mode: mode,
            queuedAt: queuedAt,
            expiresAt: expiresAt);

        search.RecordStateChanged();

        return Result.Success(search);
    }

    public Result Matched(MatchmakingTimestamp now)
    {
        if (Status == MatchSearchStatus.Matched)
        {
            return Result.Success();
        }

        if (!IsActive(now))
        {
            return Result.Failure(Error.Conflict("Only an active search can be marked as matched."));
        }

        Status = MatchSearchStatus.Matched;

        RecordStateChanged();

        return Result.Success();
    }

    public Result MatchWith(MatchSearch opponent, MatchmakingTimestamp now)
    {
        if (Id == opponent.Id
            || PlayerId == opponent.PlayerId
            || Mode != opponent.Mode
            || !IsActive(now)
            || !opponent.IsActive(now))
        {
            return Result.Failure(Error.Conflict("Only two active searches from different players can be matched."));
        }

        Status = MatchSearchStatus.Matched;
        opponent.Status = MatchSearchStatus.Matched;
        RecordStateChanged();
        opponent.RecordStateChanged();
        AddDomainEvent(new MatchFound(
            FirstMatchSearchId: Id.Value,
            FirstPlayerId: PlayerId.Value,
            FirstBuildId: BuildId.Value,
            FirstRating: Rating.Value,
            SecondMatchSearchId: opponent.Id.Value,
            SecondPlayerId: opponent.PlayerId.Value,
            SecondBuildId: opponent.BuildId.Value,
            SecondRating: opponent.Rating.Value,
            Mode: Mode.Name,
            MatchedAt: now.Value));

        return Result.Success();
    }

    public Result AssignGame(GameId gameId)
    {
        if (GameId == gameId)
        {
            return Result.Success();
        }

        if (Status != MatchSearchStatus.Matched || GameId is not null)
        {
            return Result.Failure(Error.Conflict("Only a matched search without a game can be assigned a game."));
        }

        GameId = gameId;

        RecordStateChanged();

        return Result.Success();
    }

    public Result Close()
    {
        if (Status == MatchSearchStatus.Closed)
        {
            return Result.Success();
        }

        if (Status != MatchSearchStatus.Matched)
        {
            return Result.Failure(Error.Conflict("Only a matched search can be closed."));
        }

        Status = MatchSearchStatus.Closed;

        RecordStateChanged();

        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status == MatchSearchStatus.Cancelled)
        {
            return Result.Success();
        }

        if (Status != MatchSearchStatus.Searching)
        {
            return Result.Failure(Error.Conflict("Only a searching search can be cancelled."));
        }

        Status = MatchSearchStatus.Cancelled;

        RecordStateChanged();

        return Result.Success();
    }

    public Result Expire(MatchmakingTimestamp now)
    {
        if (Status == MatchSearchStatus.Expired)
        {
            return Result.Success();
        }

        if (Status != MatchSearchStatus.Searching || now.Value < ExpiresAt.Value)
        {
            return Result.Failure(Error.Conflict("Only an overdue search in Searching status can expire."));
        }

        Status = MatchSearchStatus.Expired;

        RecordStateChanged();

        return Result.Success();
    }

    private bool IsActive(MatchmakingTimestamp now)
    {
        return Status == MatchSearchStatus.Searching
            && now.Value >= QueuedAt.Value
            && now.Value < ExpiresAt.Value;
    }

    private void RecordStateChanged()
    {
        AddDomainEvent(new MatchSearchStateChanged(
            MatchSearchId: Id.Value,
            PlayerId: PlayerId.Value,
            Status: Status.Name,
            GameId: GameId?.Value));
    }
}
