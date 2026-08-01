using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes;

namespace PANiXiDA.TacticalHeroes.Compendium.IntegrationTests.Heroes;

internal static class IntegrationTestData
{
    internal static Faction CreateFaction()
    {
        return Faction.Create(
            "Northern Alliance",
            "Defenders of the north.").Value;
    }

    internal static Hero CreateHero(
        Faction faction,
        string name = "Orrin")
    {
        return Hero.Create(
            name: name,
            description: $"{name} description.",
            attack: 8,
            defense: 6,
            minimumDamage: 3,
            maximumDamage: 7,
            initiative: 10.5,
            morale: 4,
            luck: 2,
            factionId: faction.Id.Value).Value;
    }
}
