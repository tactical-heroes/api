namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches;

public readonly partial record struct MatchSearchId : IStronglyTypedId
{
    private MatchSearchId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static MatchSearchId New()
    {
        return new MatchSearchId(Guid.CreateVersion7());
    }

    public static Result<MatchSearchId> Create(Guid value)
    {
        return value == Guid.Empty
            ? Result.Failure<MatchSearchId>(Error.Validation("MatchSearch id cannot be empty."))
            : Result.Success(new MatchSearchId(value));
    }
}
