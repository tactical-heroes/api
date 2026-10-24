using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Battle.Domain.Battles.ValueObjects;

public sealed partial class DamageResult : ValueObject
{
    private DamageResult(Damage calculatedDamage, Damage appliedDamage, int killedCount, UnitStackState remainingStack)
    {
        CalculatedDamage = calculatedDamage;
        AppliedDamage = appliedDamage;
        KilledCount = killedCount;
        RemainingStack = remainingStack;
    }

    public Damage CalculatedDamage { get; }
    public Damage AppliedDamage { get; }
    public int KilledCount { get; }
    public UnitStackState RemainingStack { get; }

    public static Result<DamageResult> Create(
        Damage calculatedDamage,
        Damage appliedDamage,
        int killedCount,
        UnitStackState remainingStack)
    {
        return appliedDamage.Value > calculatedDamage.Value || killedCount < 0
            ? Result.Failure<DamageResult>(Error.Validation("Applied damage and casualties must be valid."))
            : Result.Success(new DamageResult(calculatedDamage, appliedDamage, killedCount, remainingStack));
    }
}
