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
            name: "  Orrin  ",
            description: "  A seasoned northern commander.  ",
            attack: 8,
            defense: 6,
            minimumDamage: 3,
            maximumDamage: 7,
            initiative: 10.5,
            morale: 4,
            luck: 2,
            factionId: factionId);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.Value.Version.ShouldBe(7);
        result.Value.Name.Value.ShouldBe("Orrin");
        result.Value.Description.Value.ShouldBe("A seasoned northern commander.");
        result.Value.Stats.Attack.ShouldBe(8);
        result.Value.Stats.Defense.ShouldBe(6);
        result.Value.Stats.MinimumDamage.ShouldBe(3);
        result.Value.Stats.MaximumDamage.ShouldBe(7);
        result.Value.Stats.Initiative.ShouldBe(10.5);
        result.Value.Morale.Value.ShouldBe(4);
        result.Value.Luck.Value.ShouldBe(2);
        result.Value.FactionId.Value.ShouldBe(factionId);
    }

    [Fact(DisplayName = "Hero should reject details when values are invalid")]
    public void Create_Should_ReturnValidationFailure_When_ValuesAreInvalid()
    {
        var result = Hero.Create(
            name: string.Empty,
            description: string.Empty,
            attack: -1,
            defense: -1,
            minimumDamage: 8,
            maximumDamage: 7,
            initiative: double.NaN,
            morale: HeroMorale.Maximum + 1,
            luck: HeroLuck.Minimum - 1,
            factionId: Guid.Empty);

        result.IsFailure.ShouldBeTrue();
        var fields = result.Errors
            .Select(error => error.Metadata.GetValueOrDefault(Error.FieldMetadataKey))
            .ToArray();
        fields.ShouldContain(nameof(HeroName));
        fields.ShouldContain(nameof(HeroDescription));
        fields.ShouldContain(nameof(HeroCombatStats.Attack));
        fields.ShouldContain(nameof(HeroCombatStats.Defense));
        fields.ShouldContain(nameof(HeroCombatStats.MaximumDamage));
        fields.ShouldContain(nameof(HeroCombatStats.Initiative));
        fields.ShouldContain(nameof(HeroMorale));
        fields.ShouldContain(nameof(HeroLuck));
    }

    [Fact(DisplayName = "Hero should update all details when values are valid")]
    public void Update_Should_ReplaceDetails_When_ValuesAreValid()
    {
        var hero = CreateHero();
        var factionId = Guid.CreateVersion7();

        var result = hero.Update(
            name: "Elara",
            description: "An agile vanguard commander.",
            attack: 10,
            defense: 7,
            minimumDamage: 4,
            maximumDamage: 9,
            initiative: 12.25,
            morale: 5,
            luck: 3,
            factionId: factionId);

        result.IsSuccess.ShouldBeTrue();
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

    [Fact(DisplayName = "Hero should preserve details when value is invalid")]
    public void Update_Should_PreserveDetails_When_ValueIsInvalid()
    {
        var hero = CreateHero();
        var originalFactionId = hero.FactionId;

        var result = hero.Update(
            name: "Elara",
            description: "An agile vanguard commander.",
            attack: -1,
            defense: 7,
            minimumDamage: 4,
            maximumDamage: 9,
            initiative: 12.25,
            morale: 5,
            luck: 3,
            factionId: Guid.CreateVersion7());

        result.IsFailure.ShouldBeTrue();
        hero.Name.Value.ShouldBe("Orrin");
        hero.Description.Value.ShouldBe("A seasoned northern commander.");
        hero.Stats.Attack.ShouldBe(8);
        hero.Stats.Defense.ShouldBe(6);
        hero.Stats.MinimumDamage.ShouldBe(3);
        hero.Stats.MaximumDamage.ShouldBe(7);
        hero.Stats.Initiative.ShouldBe(10.5);
        hero.Morale.Value.ShouldBe(4);
        hero.Luck.Value.ShouldBe(2);
        hero.FactionId.ShouldBe(originalFactionId);
    }

    private static Hero CreateHero()
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
            factionId: Guid.CreateVersion7()).Value;
    }
}
