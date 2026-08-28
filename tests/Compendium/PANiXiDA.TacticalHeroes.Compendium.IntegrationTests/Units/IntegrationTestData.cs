using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Units;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Units.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.IntegrationTests.Units;

internal static class IntegrationTestData
{
    internal static Faction CreateFaction()
    {
        return Faction.Create(
            "Northern Alliance",
            "Defenders of the north.").Value;
    }

    internal static Unit CreateUnit(
        Faction faction,
        string name = "Archer")
    {
        return Unit.Create(new UnitAttributes
        {
            Name = name,
            Description = $"{name} description.",
            CombatStats = new UnitCombatStatsInput
            {
                Attack = 8,
                Defense = 4,
                Health = 12,
                MinimumDamage = 3,
                MaximumDamage = 5,
                Initiative = 10.5,
                Speed = 6,
                Shots = 12,
                RangedAttackRange = 8
            },
            Morale = 2,
            Luck = 1,
            FactionId = faction.Id.Value
        }).Value;
    }
}
