namespace PANiXiDA.TacticalHeroes.Battle.Domain.Factions;

public readonly partial record struct FactionId : IStronglyTypedId
{
    private FactionId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static FactionId New()
    {
        return new FactionId(Guid.CreateVersion7());
    }

    public static Result<FactionId> Create(Guid value)
    {
        return value == Guid.Empty
            ? Result.Failure<FactionId>(Error.Validation("Faction id cannot be empty."))
            : Result.Success(new FactionId(value));
    }
}
