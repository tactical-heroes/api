namespace PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Heroes.ValueObjects;

public sealed partial class HeroCombatStats : ValueObject
{
    private HeroCombatStats(
        int attack,
        int defense,
        int minimumDamage,
        int maximumDamage,
        double initiative,
        int morale,
        int luck)
    {
        Attack = attack;
        Defense = defense;
        MinimumDamage = minimumDamage;
        MaximumDamage = maximumDamage;
        Initiative = initiative;
        Morale = morale;
        Luck = luck;
    }

    public int Attack { get; }
    public int Defense { get; }
    public int MinimumDamage { get; }
    public int MaximumDamage { get; }
    public double Initiative { get; }
    public int Morale { get; }
    public int Luck { get; }

    public static Result<HeroCombatStats> Create(
        int attack,
        int defense,
        int minimumDamage,
        int maximumDamage,
        double initiative,
        int morale,
        int luck)
    {
        if (attack < 0
            || defense < 0
            || minimumDamage < 0
            || maximumDamage < minimumDamage
            || !double.IsFinite(initiative)
            || initiative < 0
            || morale is < 0 or > 5
            || luck is < 0 or > 5)
        {
            return Result.Failure<HeroCombatStats>(Error.Validation("Hero stats must contain valid combat values."));
        }

        return Result.Success(new HeroCombatStats(
            attack: attack,
            defense: defense,
            minimumDamage: minimumDamage,
            maximumDamage: maximumDamage,
            initiative: initiative,
            morale: morale,
            luck: luck));
    }
}
