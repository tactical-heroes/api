namespace PANiXiDA.TacticalHeroes.Battle.Domain.Battles.ValueObjects;

public sealed partial class Damage : ValueObject
{
    private Damage(long value)
    {
        Value = value;
    }

    public long Value { get; }

    public static Result<Damage> Create(long value)
    {
        return value < 0
            ? Result.Failure<Damage>(Error.Validation("Damage cannot be negative."))
            : Result.Success(new Damage(value));
    }
}
