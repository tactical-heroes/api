using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.ValueObjects;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Units;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Units.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.IntegrationTests.Units;

internal static class IntegrationTestData
{
    internal static Faction CreateFaction()
    {
        return Faction.Create(
            name: FactionName.Create(value: "Northern Alliance").Value,
            description: FactionDescription.Create(value: "Defenders of the north.").Value);
    }

    internal static Unit CreateUnit(
        Faction faction,
        string name = "Archer")
    {
        return Unit.Create(
            name: UnitName.Create(value: name).Value,
            description: UnitDescription.Create(value: $"{name} description.").Value,
            stats: UnitCombatStats.Create(new UnitCombatStatsInput
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
            }).Value,
            morale: UnitMorale.Create(value: 2).Value,
            luck: UnitLuck.Create(value: 1).Value,
            factionId: faction.Id);
    }
}
