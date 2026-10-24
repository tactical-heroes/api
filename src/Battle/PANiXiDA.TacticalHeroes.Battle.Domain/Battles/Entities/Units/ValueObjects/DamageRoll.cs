namespace PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units.ValueObjects;

public sealed partial class DamageRoll : ValueObject
{
    private DamageRoll(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public static Result<DamageRoll> Create(int value)
    {
        return value < 0
            ? Result.Failure<DamageRoll>(Error.Validation("Damage roll cannot be negative."))
            : Result.Success(new DamageRoll(value));
    }
}
