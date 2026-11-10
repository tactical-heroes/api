namespace PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units.ValueObjects;

public sealed partial class UnitTraits : ValueObject
{
    private UnitTraits(double initiative, int speed, int morale, int luck, int? shots, int? rangedAttackRange)
    {
        Initiative = initiative;
        Speed = speed;
        Morale = morale;
        Luck = luck;
        Shots = shots;
        RangedAttackRange = rangedAttackRange;
    }

    public double Initiative { get; }
    public int Speed { get; }
    public int Morale { get; }
    public int Luck { get; }
    public int? Shots { get; }
    public int? RangedAttackRange { get; }

    public static Result<UnitTraits> Create(
        double initiative,
        int speed,
        int morale,
        int luck,
        int? shots,
        int? rangedAttackRange)
    {
        if (!double.IsFinite(initiative)
            || initiative < 0
            || speed < 0
            || morale is < 0 or > 5
            || luck is < 0 or > 5
            || shots.HasValue != rangedAttackRange.HasValue
            || shots is < 0
            || rangedAttackRange is <= 0)
        {
            return Result.Failure<UnitTraits>(Error.Validation("Unit traits must contain valid combat values."));
        }

        return Result.Success(new UnitTraits(
            initiative: initiative,
            speed: speed,
            morale: morale,
            luck: luck,
            shots: shots,
            rangedAttackRange: rangedAttackRange));
    }
}
