namespace PANiXiDA.TacticalHeroes.Games.Domain.MatchSearches;

public readonly partial record struct MatchSearchId : IStronglyTypedId
{
    private MatchSearchId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static MatchSearchId New()
    {
        return new MatchSearchId(value: Guid.CreateVersion7());
    }

    public static Result<MatchSearchId> Create(Guid value)
    {
        return value == Guid.Empty
            ? Result.Failure<MatchSearchId>(
                error: Error.Validation(message: "Match search id cannot be empty."))
            : Result.Success(value: new MatchSearchId(value: value));
    }
}
