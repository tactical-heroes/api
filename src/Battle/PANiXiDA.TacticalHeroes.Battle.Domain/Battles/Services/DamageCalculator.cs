using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units.ValueObjects;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Services;

public static class DamageCalculator
{
    private const decimal StatsModifierCoefficient = 0.05m;

    public static Result<Damage> Calculate(
        UnitCombatStats attackerStats,
        UnitStackState attackerStack,
        DamageRange damageRange,
        UnitCombatStats defenderStats,
        DamageRoll roll)
    {
        if (attackerStack.Count == 0 || roll.Value < damageRange.Minimum || roll.Value > damageRange.Maximum)
        {
            return Result.Failure<Damage>(Error.Validation(
                "A living attacker and a roll within its damage range are required."));
        }

        var difference = attackerStats.Attack - defenderStats.Defense;
        var statsModifier = 1 + StatsModifierCoefficient * Math.Abs(difference);
        var baseDamage = (decimal)roll.Value * attackerStack.Count;
        var modifiedDamage = difference >= 0 ? baseDamage * statsModifier : baseDamage / statsModifier;
        var roundedDamage = decimal.Round(d: modifiedDamage, decimals: 0, mode: MidpointRounding.AwayFromZero);

        return roundedDamage > long.MaxValue
            ? Result.Failure<Damage>(Error.Validation("Calculated damage exceeds the supported range."))
            : Damage.Create((long)roundedDamage);
    }
}
