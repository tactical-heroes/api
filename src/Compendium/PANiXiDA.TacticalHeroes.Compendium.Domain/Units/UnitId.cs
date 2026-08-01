namespace PANiXiDA.TacticalHeroes.Compendium.Domain.Units;

public readonly record struct UnitId(Guid Value) : IStronglyTypedId
{
    public static UnitId New()
    {
        return new UnitId(Value: Guid.CreateVersion7());
    }

    public static Result<UnitId> Create(Guid value)
    {
        return value == Guid.Empty
            ? Result.Failure<UnitId>(
                error: Error.Validation(message: "Unit id cannot be empty."))
            : Result.Success(value: new UnitId(Value: value));
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
