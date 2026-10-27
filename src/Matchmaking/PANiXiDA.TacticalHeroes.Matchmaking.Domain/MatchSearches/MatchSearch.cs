using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Builds;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Matches;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Events;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.ValueObjects;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Players;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches;

public sealed class MatchSearch : AggregateRoot<MatchSearchId>
{
    private MatchSearch(
        MatchSearchId id,
        PlayerId playerId,
        BuildId buildId,
        RatingSnapshot rating,
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
    public RatingSnapshot Rating { get; private set; }
    public MatchmakingMode Mode { get; private set; }
    public MatchmakingTimestamp QueuedAt { get; private set; }
    public MatchmakingTimestamp ExpiresAt { get; private set; }
    public MatchSearchStatus Status { get; private set; }
    public MatchId? MatchId { get; private set; }

    public static Result<MatchSearch> Create(
        PlayerId playerId,
        BuildId buildId,
        RatingSnapshot rating,
        MatchmakingMode mode,
        MatchmakingTimestamp queuedAt,
        MatchmakingTimestamp expiresAt)
    {
        if (expiresAt.Value <= queuedAt.Value)
        {
            return Result.Failure<MatchSearch>(Error.Validation("Search expiration must follow queue entry."));
        }

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

    public Result Reserve(MatchId matchId, MatchmakingTimestamp now)
    {
        if (matchId.Value == Guid.Empty)
        {
            return Result.Failure(Error.Validation("Match id cannot be empty."));
        }

        if (Status == MatchSearchStatus.Reserved && MatchId == matchId)
        {
            return Result.Success();
        }

        if (Status != MatchSearchStatus.Searching || now.Value < QueuedAt.Value || now.Value >= ExpiresAt.Value)
        {
            return Result.Failure(Error.Conflict("Only an active search can be reserved."));
        }

        MatchId = matchId;
        Status = MatchSearchStatus.Reserved;
        RecordStateChanged();

        return Result.Success();
    }

    public Result MarkAssigned(MatchId matchId)
    {
        if (MatchId != matchId || matchId.Value == Guid.Empty)
        {
            return Result.Failure(Error.Conflict("Search does not belong to this match."));
        }

        if (Status == MatchSearchStatus.Assigned)
        {
            return Result.Success();
        }

        if (Status != MatchSearchStatus.Reserved)
        {
            return Result.Failure(Error.Conflict("Only a reserved search can be assigned to a game."));
        }

        Status = MatchSearchStatus.Assigned;
        RecordStateChanged();

        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status == MatchSearchStatus.Cancelled)
        {
            return Result.Success();
        }

        if (Status != MatchSearchStatus.Searching && Status != MatchSearchStatus.Reserved)
        {
            return Result.Failure(Error.Conflict("Only a searching or reserved search can be cancelled."));
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

    private void RecordStateChanged()
    {
        AddDomainEvent(new MatchSearchStateChanged(
            MatchSearchId: Id.Value,
            PlayerId: PlayerId.Value,
            Status: Status.Name,
            MatchId: MatchId?.Value));
    }
}
