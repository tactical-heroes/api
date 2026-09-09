using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.ValueObjects;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.IntegrationTests.Heroes;

internal static class IntegrationTestData
{
    internal static Faction CreateFaction()
    {
        return Faction.Create(
            name: FactionName.Create(value: "Northern Alliance").Value,
            description: FactionDescription.Create(value: "Defenders of the north.").Value);
    }

    internal static Hero CreateHero(
        Faction faction,
        string name = "Orrin")
    {
        return Hero.Create(
            name: HeroName.Create(value: name).Value,
            description: HeroDescription.Create(value: $"{name} description.").Value,
            stats: HeroCombatStats.Create(
                attack: 8,
                defense: 6,
                minimumDamage: 3,
                maximumDamage: 7,
                initiative: 10.5).Value,
            morale: HeroMorale.Create(value: 4).Value,
            luck: HeroLuck.Create(value: 2).Value,
            factionId: faction.Id);
    }
}
