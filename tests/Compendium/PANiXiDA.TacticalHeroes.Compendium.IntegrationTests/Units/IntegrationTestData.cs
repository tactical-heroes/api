using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Units;

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
        return Unit.Create(
            name: name,
            description: $"{name} description.",
            attack: 8,
            defense: 4,
            health: 12,
            minimumDamage: 3,
            maximumDamage: 5,
            initiative: 10.5,
            speed: 6,
            shots: 12,
            rangedAttackRange: 8,
            morale: 2,
            luck: 1,
            factionId: faction.Id.Value).Value;
    }
}
