using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units.ValueObjects;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units;

public sealed class Unit : Entity<UnitId>
{
    private Unit(UnitId id) : base(id)
    {
        Stats = null!;
        DamageRange = null!;
        StackState = null!;
    }

    private Unit(UnitId id, UnitCombatStats stats, DamageRange damageRange, UnitStackState stackState) : this(id)
    {
        Stats = stats;
        DamageRange = damageRange;
        StackState = stackState;
    }

    public BattleId? BattleId { get; private set; }
    public UnitCombatStats Stats { get; private set; }
    public DamageRange DamageRange { get; private set; }
    public UnitStackState StackState { get; private set; }

    public int Count => StackState.Count;
    public int CurrentUnitHealth => StackState.CurrentUnitHealth;
    public bool IsAlive => Count > 0;
    public long TotalHealth => IsAlive ? (long)(Count - 1) * Stats.MaxHealth + CurrentUnitHealth : 0;

    public static Result<Unit> Create(UnitCombatStats stats, DamageRange damageRange, UnitStackState stackState)
    {
        return stackState.CurrentUnitHealth > stats.MaxHealth
            ? Result.Failure<Unit>(Error.Validation("Current unit health cannot exceed maximum health."))
            : Result.Success(new Unit(
                id: UnitId.New(),
                stats: stats,
                damageRange: damageRange,
                stackState: stackState));
    }

    internal void AssignToBattle(BattleId battleId)
    {
        if (battleId.Value == Guid.Empty)
        {
            throw new ArgumentException("Battle id cannot be empty.", nameof(battleId));
        }

        if (BattleId.HasValue)
        {
            throw new InvalidOperationException("Unit already belongs to a battle.");
        }

        BattleId = battleId;
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
