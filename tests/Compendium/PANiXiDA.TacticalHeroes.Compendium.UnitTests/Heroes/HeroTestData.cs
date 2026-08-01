using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Create;
using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.GetDetails;
using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Update;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Heroes;

internal static class HeroTestData
{
    internal static Faction CreateFaction()
    {
        return Faction.Create(
            "Northern Alliance",
            "Defenders of the north.").Value;
    }

    internal static Hero CreateHero(Faction faction)
    {
        return Hero.Create(
            name: "Orrin",
            description: "A seasoned northern commander.",
            attack: 8,
            defense: 6,
            minimumDamage: 3,
            maximumDamage: 7,
            initiative: 10.5,
            morale: 4,
            luck: 2,
            factionId: faction.Id.Value).Value;
    }

    internal static CreateHeroCommand CreateCommand(Guid factionId)
    {
        return new CreateHeroCommand(
            Name: "Orrin",
            Description: "A seasoned northern commander.",
            Attack: 8,
            Defense: 6,
            MinimumDamage: 3,
            MaximumDamage: 7,
            Initiative: 10.5,
            Morale: 4,
            Luck: 2,
            FactionId: factionId);
    }

    internal static UpdateHeroCommand CreateUpdateCommand(
        Guid heroId,
        Guid factionId)
    {
        return new UpdateHeroCommand(
            Id: heroId,
            Name: "Elara",
            Description: "An agile vanguard commander.",
            Attack: 10,
            Defense: 7,
            MinimumDamage: 4,
            MaximumDamage: 9,
            Initiative: 12.25,
            Morale: 5,
            Luck: 3,
            FactionId: factionId);
    }

    internal static HeroDetailsReadModel CreateDetailsReadModel(Guid factionId)
    {
        return new HeroDetailsReadModel(
            Id: Guid.CreateVersion7(),
            Name: "Orrin",
            Description: "A seasoned northern commander.",
            Attack: 8,
            Defense: 6,
            MinimumDamage: 3,
            MaximumDamage: 7,
            Initiative: 10.5,
            Morale: 4,
            Luck: 2,
            FactionId: factionId);
    }
}
