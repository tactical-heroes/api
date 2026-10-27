namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.Matches;

public readonly partial record struct MatchId : IStronglyTypedId
{
    private MatchId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static MatchId New()
    {
        return new MatchId(Guid.CreateVersion7());
    }

    public static Result<MatchId> Create(Guid value)
    {
        return value == Guid.Empty
            ? Result.Failure<MatchId>(Error.Validation("Match id cannot be empty."))
            : Result.Success(new MatchId(value));
    }
}
