namespace PANiXiDA.TacticalHeroes.Compendium.Domain.Units;

public readonly partial record struct UnitId : IStronglyTypedId
{
    private UnitId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static UnitId New()
    {
        return new UnitId(value: Guid.CreateVersion7());
    }

    public static Result<UnitId> Create(Guid value)
    {
        return value == Guid.Empty
            ? Result.Failure<UnitId>(
                error: Error.Validation(message: "Unit id cannot be empty."))
            : Result.Success(value: new UnitId(value: value));
    }
}
