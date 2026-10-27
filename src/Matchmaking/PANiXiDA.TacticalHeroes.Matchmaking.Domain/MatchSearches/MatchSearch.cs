using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Builds;
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
        Raiting rating,
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
    public Raiting Rating { get; private set; }
    public MatchmakingMode Mode { get; private set; }
    public MatchmakingTimestamp QueuedAt { get; private set; }
    public MatchmakingTimestamp ExpiresAt { get; private set; }
    public MatchSearchStatus Status { get; private set; }

    public static Result<MatchSearch> Create(
        PlayerId playerId,
        BuildId buildId,
        Raiting rating,
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

    public Result MatchWith(MatchSearch other, MatchmakingTimestamp now)
    {
        if (Id == other.Id || PlayerId == other.PlayerId || Mode != other.Mode)
        {
            return Result.Failure(Error.Validation("A duel requires distinct players and searches in the same mode."));
        }

        if (!IsActive(now) || !other.IsActive(now))
        {
            return Result.Failure(Error.Conflict("Both searches must be active."));
        }

        Status = MatchSearchStatus.Matched;
        other.Status = MatchSearchStatus.Matched;

        RecordStateChanged();

        other.RecordStateChanged();

        AddDomainEvent(new MatchFound(
            FirstMatchSearchId: Id.Value,
            FirstPlayerId: PlayerId.Value,
            FirstBuildId: BuildId.Value,
            FirstRating: Rating.Value,
            SecondMatchSearchId: other.Id.Value,
            SecondPlayerId: other.PlayerId.Value,
            SecondBuildId: other.BuildId.Value,
            SecondRating: other.Rating.Value,
            Mode: Mode.Name,
            MatchedAt: now.Value));

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
            Status: Status.Name));
    }
}
