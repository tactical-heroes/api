using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Domain.Heroes;

public sealed class HeroTests
{
    [Fact(DisplayName = "Hero should create normalized details when values are valid")]
    public void Create_Should_ReturnHero_When_ValuesAreValid()
    {
        var factionId = Guid.CreateVersion7();

        var result = Hero.Create(
            name: HeroName.Create(value: "  Orrin  ").Value,
            description: HeroDescription.Create(value: "  A seasoned northern commander.  ").Value,
            stats: HeroCombatStats.Create(
                attack: 8,
                defense: 6,
                minimumDamage: 3,
                maximumDamage: 7,
                initiative: 10.5).Value,
            morale: HeroMorale.Create(value: 4).Value,
            luck: HeroLuck.Create(value: 2).Value,
            factionId: FactionId.Create(value: factionId).Value);

        result.Id.Value.Version.ShouldBe(7);
        result.Name.Value.ShouldBe("Orrin");
        result.Description.Value.ShouldBe("A seasoned northern commander.");
        result.Stats.Attack.ShouldBe(8);
        result.Stats.Defense.ShouldBe(6);
        result.Stats.MinimumDamage.ShouldBe(3);
        result.Stats.MaximumDamage.ShouldBe(7);
        result.Stats.Initiative.ShouldBe(10.5);
        result.Morale.Value.ShouldBe(4);
        result.Luck.Value.ShouldBe(2);
        result.FactionId.Value.ShouldBe(factionId);
    }

    [Fact(DisplayName = "Hero should update all details when values are valid")]
    public void Update_Should_ReplaceDetails_When_ValuesAreValid()
    {
        var hero = CreateHero();
        var factionId = Guid.CreateVersion7();

        hero.Update(
            name: HeroName.Create(value: "Elara").Value,
            description: HeroDescription.Create(value: "An agile vanguard commander.").Value,
            stats: HeroCombatStats.Create(
                attack: 10,
                defense: 7,
                minimumDamage: 4,
                maximumDamage: 9,
                initiative: 12.25).Value,
            morale: HeroMorale.Create(value: 5).Value,
            luck: HeroLuck.Create(value: 3).Value,
            factionId: FactionId.Create(value: factionId).Value);

        hero.Name.Value.ShouldBe("Elara");
        hero.Description.Value.ShouldBe("An agile vanguard commander.");
        hero.Stats.Attack.ShouldBe(10);
        hero.Stats.Defense.ShouldBe(7);
        hero.Stats.MinimumDamage.ShouldBe(4);
        hero.Stats.MaximumDamage.ShouldBe(9);
        hero.Stats.Initiative.ShouldBe(12.25);
        hero.Morale.Value.ShouldBe(5);
        hero.Luck.Value.ShouldBe(3);
        hero.FactionId.Value.ShouldBe(factionId);
    }

    private static Hero CreateHero()
    {
        return Hero.Create(
            name: HeroName.Create(value: "Orrin").Value,
            description: HeroDescription.Create(value: "A seasoned northern commander.").Value,
            stats: HeroCombatStats.Create(
                attack: 8,
                defense: 6,
                minimumDamage: 3,
                maximumDamage: 7,
                initiative: 10.5).Value,
            morale: HeroMorale.Create(value: 4).Value,
            luck: HeroLuck.Create(value: 2).Value,
            factionId: FactionId.Create(value: Guid.CreateVersion7()).Value);
    }
}
