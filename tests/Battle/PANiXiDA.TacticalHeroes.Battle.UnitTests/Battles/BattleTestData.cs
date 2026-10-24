using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Battle.UnitTests.Battles;

internal static class BattleTestData
{
    internal static Unit CreateUnit(
        int attack = 0,
        int defense = 0,
        int maxHealth = 10,
        int minimumDamage = 1,
        int maximumDamage = 1,
        int count = 3,
        int? currentHealth = null)
    {
        return Unit.Create(
            UnitCombatStats.Create(attack, defense, maxHealth).Value,
            DamageRange.Create(minimumDamage, maximumDamage).Value,
            UnitStackState.Create(count, currentHealth ?? (count == 0 ? 0 : maxHealth)).Value).Value;
    }
}
