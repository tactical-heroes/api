namespace PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units.ValueObjects;

public sealed partial class UnitCombatStats : ValueObject
{
    private UnitCombatStats(int attack, int defense, int maxHealth)
    {
        Attack = attack;
        Defense = defense;
        MaxHealth = maxHealth;
    }

    public int Attack { get; }
    public int Defense { get; }
    public int MaxHealth { get; }

    public static Result<UnitCombatStats> Create(int attack, int defense, int maxHealth)
    {
        return attack < 0 || defense < 0 || maxHealth <= 0
            ? Result.Failure<UnitCombatStats>(Error.Validation(
                "Attack and defense must be non-negative, and maximum health must be positive."))
            : Result.Success(new UnitCombatStats(attack: attack, defense: defense, maxHealth: maxHealth));
    }
}
