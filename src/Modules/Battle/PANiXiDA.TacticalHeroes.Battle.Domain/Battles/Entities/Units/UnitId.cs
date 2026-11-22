namespace PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units;

public readonly partial record struct UnitId : IStronglyTypedId
{
    private UnitId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static UnitId New()
    {
        return new UnitId(Guid.CreateVersion7());
    }

    public static Result<UnitId> Create(Guid value)
    {
        return value == Guid.Empty
            ? Result.Failure<UnitId>(Error.Validation("Unit id cannot be empty."))
            : Result.Success(new UnitId(value));
    }
}
