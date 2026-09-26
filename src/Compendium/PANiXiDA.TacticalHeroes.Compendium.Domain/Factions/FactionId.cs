namespace PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;

public readonly partial record struct FactionId : IStronglyTypedId
{
    private FactionId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static FactionId New()
    {
        return new FactionId(value: Guid.CreateVersion7());
    }

    public static Result<FactionId> Create(Guid value)
    {
        return value == Guid.Empty
            ? Result.Failure<FactionId>(
                error: Error.Validation(message: "Faction id cannot be empty."))
            : Result.Success(value: new FactionId(value: value));
    }
}
