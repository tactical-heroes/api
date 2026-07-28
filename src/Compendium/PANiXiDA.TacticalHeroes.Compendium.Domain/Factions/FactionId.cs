namespace PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;

public readonly record struct FactionId(Guid Value) : IStronglyTypedId
{
    public static FactionId New()
    {
        return new FactionId(Value: Guid.CreateVersion7());
    }

    public static Result<FactionId> Create(Guid value)
    {
        return value == Guid.Empty
            ? Result.Failure<FactionId>(
                error: Error.Validation(message: "Faction id cannot be empty."))
            : Result.Success(value: new FactionId(Value: value));
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
