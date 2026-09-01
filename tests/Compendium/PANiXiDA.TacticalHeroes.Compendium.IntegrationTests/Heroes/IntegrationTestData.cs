using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes.ValueObjects;

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
        return Hero.Create(new HeroAttributes
        {
            Name = name,
            Description = $"{name} description.",
            Attack = 8,
            Defense = 6,
            MinimumDamage = 3,
            MaximumDamage = 7,
            Initiative = 10.5,
            Morale = 4,
            Luck = 2,
            FactionId = faction.Id.Value
        }).Value;
    }
}
