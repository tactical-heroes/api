using PANiXiDA.TacticalHeroes.Compendium.Domain.Units;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Units.ValueObjects;
using PANiXiDA.TacticalHeroes.Compendium.UnitTests.Units;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Domain.Units;

public sealed class UnitTests
{
    [Fact(DisplayName = "Unit should create normalized ranged details when values are valid")]
    public void Create_Should_ReturnRangedUnit_When_ValuesAreValid()
    {
        var faction = UnitTestData.CreateFaction();

        var result = Unit.Create(
            name: "  Archer  ",
            description: "  A disciplined ranged unit.  ",
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
            factionId: faction.Id.Value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.Value.Version.ShouldBe(7);
        result.Value.Name.Value.ShouldBe("Archer");
        result.Value.Description.Value.ShouldBe("A disciplined ranged unit.");
        result.Value.Stats.Attack.ShouldBe(8);
        result.Value.Stats.Defense.ShouldBe(4);
        result.Value.Stats.Health.ShouldBe(12);
        result.Value.Stats.MinimumDamage.ShouldBe(3);
        result.Value.Stats.MaximumDamage.ShouldBe(5);
        result.Value.Stats.Initiative.ShouldBe(10.5);
        result.Value.Stats.Speed.ShouldBe(6);
        result.Value.Stats.Shots.ShouldBe(12);
        result.Value.Stats.RangedAttackRange.ShouldBe(8);
        result.Value.Morale.Value.ShouldBe(2);
        result.Value.Luck.Value.ShouldBe(1);
        result.Value.FactionId.ShouldBe(faction.Id);
    }

    [Fact(DisplayName = "Unit should create melee details when ranged values are omitted")]
    public void Create_Should_ReturnMeleeUnit_When_RangedValuesAreOmitted()
    {
        var faction = UnitTestData.CreateFaction();

        var result = Unit.Create(
            name: "Swordsman",
            description: "A disciplined melee unit.",
            attack: 6,
            defense: 7,
            health: 18,
            minimumDamage: 2,
            maximumDamage: 4,
            initiative: 8,
            speed: 5,
            shots: null,
            rangedAttackRange: null,
            morale: 1,
            luck: 0,
            factionId: faction.Id.Value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Stats.Shots.ShouldBeNull();
        result.Value.Stats.RangedAttackRange.ShouldBeNull();
    }

    [Fact(DisplayName = "Unit should reject statistics when statistics are invalid")]
    public void Create_Should_ReturnValidationFailure_When_StatisticsAreInvalid()
    {
        var faction = UnitTestData.CreateFaction();

        var result = Unit.Create(
            name: "Archer",
            description: "A disciplined ranged unit.",
            attack: -1,
            defense: -1,
            health: 0,
            minimumDamage: 6,
            maximumDamage: 5,
            initiative: double.NaN,
            speed: -1,
            shots: 12,
            rangedAttackRange: null,
            morale: 6,
            luck: -1,
            factionId: faction.Id.Value);

        result.IsFailure.ShouldBeTrue();
        var fields = result.Errors
            .Select(error => error.Metadata.GetValueOrDefault(Error.FieldMetadataKey))
            .ToArray();
        fields.ShouldContain(nameof(UnitCombatStats.Attack));
        fields.ShouldContain(nameof(UnitCombatStats.Health));
        fields.ShouldContain(nameof(UnitCombatStats.MaximumDamage));
        fields.ShouldContain(nameof(UnitCombatStats.Initiative));
        fields.ShouldContain(nameof(UnitCombatStats.RangedAttackRange));
    }

    [Fact(DisplayName = "Unit should update all details when values are valid")]
    public void Update_Should_ReplaceDetails_When_ValuesAreValid()
    {
        var faction = UnitTestData.CreateFaction();
        var unit = UnitTestData.CreateUnit(faction);

        var result = unit.Update(
            name: "Marksman",
            description: "An elite ranged unit.",
            attack: 10,
            defense: 5,
            health: 14,
            minimumDamage: 4,
            maximumDamage: 7,
            initiative: 11.5,
            speed: 7,
            shots: 16,
            rangedAttackRange: 10,
            morale: 3,
            luck: 2,
            factionId: faction.Id.Value);

        result.IsSuccess.ShouldBeTrue();
        unit.Name.Value.ShouldBe("Marksman");
        unit.Stats.Attack.ShouldBe(10);
        unit.Stats.MaximumDamage.ShouldBe(7);
        unit.Stats.Shots.ShouldBe(16);
        unit.Morale.Value.ShouldBe(3);
        unit.Luck.Value.ShouldBe(2);
    }

    [Fact(DisplayName = "Unit should preserve details when value is invalid")]
    public void Update_Should_PreserveDetails_When_ValueIsInvalid()
    {
        var faction = UnitTestData.CreateFaction();
        var unit = UnitTestData.CreateUnit(faction);

        var result = unit.Update(
            name: "Marksman",
            description: "An elite ranged unit.",
            attack: -1,
            defense: 5,
            health: 14,
            minimumDamage: 4,
            maximumDamage: 7,
            initiative: 11.5,
            speed: 7,
            shots: 16,
            rangedAttackRange: 10,
            morale: 3,
            luck: 2,
            factionId: faction.Id.Value);

        result.IsFailure.ShouldBeTrue();
        unit.Name.Value.ShouldBe("Archer");
        unit.Stats.Attack.ShouldBe(8);
        unit.Stats.MaximumDamage.ShouldBe(5);
        unit.Morale.Value.ShouldBe(2);
    }
}
