using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Abstractions;

namespace PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units.ValueObjects;

public sealed partial class DamageRange : ValueObject
{
    private DamageRange(int minimum, int maximum)
    {
        Minimum = minimum;
        Maximum = maximum;
    }

    public int Minimum { get; }
    public int Maximum { get; }

    public static Result<DamageRange> Create(int minimum, int maximum)
    {
        return minimum < 0 || maximum < minimum
            ? Result.Failure<DamageRange>(Error.Validation("Damage range must be non-negative and ordered."))
            : Result.Success(new DamageRange(minimum, maximum));
    }

    public Result<DamageRoll> Roll(IRandomSource randomSource)
    {
        if (Minimum == Maximum)
        {
            return DamageRoll.Create(Minimum);
        }

        var value = randomSource.NextInt64(Minimum, (long)Maximum + 1);

        return value < Minimum || value > Maximum
            ? Result.Failure<DamageRoll>(Error.Validation("Random source returned a value outside the damage range."))
            : DamageRoll.Create((int)value);
    }
}
