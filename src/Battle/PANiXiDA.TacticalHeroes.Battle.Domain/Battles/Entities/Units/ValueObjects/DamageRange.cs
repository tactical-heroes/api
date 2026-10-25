using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Services;

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

    public int Roll()
    {
        return DamageRandomizer.Roll(Minimum, Maximum);
    }
}
