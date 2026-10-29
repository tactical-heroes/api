namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Policies;

public sealed record MatchmakingPair
{
    private MatchmakingPair(Guid firstSearchId, Guid secondSearchId)
    {
        FirstSearchId = firstSearchId;
        SecondSearchId = secondSearchId;
    }

    public Guid FirstSearchId { get; }
    public Guid SecondSearchId { get; }

    public static Result<MatchmakingPair> Create(Guid firstSearchId, Guid secondSearchId)
    {
        return firstSearchId == Guid.Empty || secondSearchId == Guid.Empty || firstSearchId == secondSearchId
            ? Result.Failure<MatchmakingPair>(Error.Validation("A pair requires two distinct non-empty search ids."))
            : Result.Success(new MatchmakingPair(firstSearchId, secondSearchId));
    }
}
