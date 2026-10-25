using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units.ValueObjects;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units;

public sealed class Unit : Entity<UnitId>
{
    private Unit(UnitId id, BattleId battleId) : base(id)
    {
        BattleId = battleId;
        Stats = null!;
        DamageRange = null!;
        StackState = null!;
    }

    private Unit(
        UnitId id,
        BattleId battleId,
        UnitCombatStats stats,
        DamageRange damageRange,
        UnitStackState stackState) : this(id, battleId)
    {
        Stats = stats;
        DamageRange = damageRange;
        StackState = stackState;
    }

    public BattleId BattleId { get; private set; }
    public UnitCombatStats Stats { get; private set; }
    public DamageRange DamageRange { get; private set; }
    public UnitStackState StackState { get; private set; }

    public int Count => StackState.Count;
    public int CurrentUnitHealth => StackState.CurrentUnitHealth;
    public bool IsAlive => Count > 0;
    public long TotalHealth => IsAlive ? (long)(Count - 1) * Stats.MaxHealth + CurrentUnitHealth : 0;

    public static Result<Unit> Create(
        BattleId battleId,
        UnitCombatStats stats,
        DamageRange damageRange,
        UnitStackState stackState)
    {
        if (battleId.Value == Guid.Empty)
        {
            return Result.Failure<Unit>(Error.Validation("Battle id cannot be empty."));
        }

        return stackState.CurrentUnitHealth > stats.MaxHealth
            ? Result.Failure<Unit>(Error.Validation("Current unit health cannot exceed maximum health."))
            : Result.Success(new Unit(
                id: UnitId.New(),
                battleId: battleId,
                stats: stats,
                damageRange: damageRange,
                stackState: stackState));
    }

    internal Result TakeDamage(Damage damage)
    {
        if (!IsAlive)
        {
            return Result.Failure(Error.Validation("A destroyed unit cannot take damage."));
        }

        var appliedDamage = Math.Min(damage.Value, TotalHealth);
        var remainingHealth = TotalHealth - appliedDamage;
        var remainingCount = remainingHealth == 0 ? 0 : (int)((remainingHealth - 1) / Stats.MaxHealth + 1);
        var currentUnitHealth = remainingHealth == 0 ? 0 : (int)((remainingHealth - 1) % Stats.MaxHealth + 1);
        StackState = UnitStackState.Create(remainingCount, currentUnitHealth).Value;

        return Result.Success();
    }
}
