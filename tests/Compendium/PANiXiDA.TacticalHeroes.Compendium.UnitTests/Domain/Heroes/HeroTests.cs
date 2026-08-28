using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Domain.Heroes;

public sealed class HeroTests
{
    [Fact(DisplayName = "Hero should create normalized details when values are valid")]
    public void Create_Should_ReturnHero_When_ValuesAreValid()
    {
        var factionId = Guid.CreateVersion7();

        var result = Hero.Create(new HeroAttributes
        {
            Name = "  Orrin  ",
            Description = "  A seasoned northern commander.  ",
            Attack = 8,
            Defense = 6,
            MinimumDamage = 3,
            MaximumDamage = 7,
            Initiative = 10.5,
            Morale = 4,
            Luck = 2,
            FactionId = factionId
        });

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
        var result = Hero.Create(new HeroAttributes
        {
            Name = string.Empty,
            Description = string.Empty,
            Attack = -1,
            Defense = -1,
            MinimumDamage = 8,
            MaximumDamage = 7,
            Initiative = double.NaN,
            Morale = HeroMorale.Maximum + 1,
            Luck = HeroLuck.Minimum - 1,
            FactionId = Guid.Empty
        });

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

        var result = hero.Update(new HeroAttributes
        {
            Name = "Elara",
            Description = "An agile vanguard commander.",
            Attack = 10,
            Defense = 7,
            MinimumDamage = 4,
            MaximumDamage = 9,
            Initiative = 12.25,
            Morale = 5,
            Luck = 3,
            FactionId = factionId
        });

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

        var result = hero.Update(new HeroAttributes
        {
            Name = "Elara",
            Description = "An agile vanguard commander.",
            Attack = -1,
            Defense = 7,
            MinimumDamage = 4,
            MaximumDamage = 9,
            Initiative = 12.25,
            Morale = 5,
            Luck = 3,
            FactionId = Guid.CreateVersion7()
        });

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
        return Hero.Create(new HeroAttributes
        {
            Name = "Orrin",
            Description = "A seasoned northern commander.",
            Attack = 8,
            Defense = 6,
            MinimumDamage = 3,
            MaximumDamage = 7,
            Initiative = 10.5,
            Morale = 4,
            Luck = 2,
            FactionId = Guid.CreateVersion7()
        }).Value;
    }
}
