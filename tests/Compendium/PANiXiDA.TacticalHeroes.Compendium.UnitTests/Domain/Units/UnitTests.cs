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
            name: UnitName.Create(value: "  Archer  ").Value,
            description: UnitDescription.Create(value: "  A disciplined ranged unit.  ").Value,
            stats: UnitCombatStats.Create(
                attack: 8,
                defense: 4,
                health: 12,
                minimumDamage: 3,
                maximumDamage: 5,
                initiative: 10.5,
                speed: 6).Value,
            rangedAttack: UnitRangedAttack.Create(
                shots: 12,
                rangedAttackRange: 8).Value,
            morale: UnitMorale.Create(value: 2).Value,
            luck: UnitLuck.Create(value: 1).Value,
            factionId: faction.Id);

        result.Id.Value.Version.ShouldBe(7);
        result.Name.Value.ShouldBe("Archer");
        result.Description.Value.ShouldBe("A disciplined ranged unit.");
        result.Stats.Attack.ShouldBe(8);
        result.Stats.Defense.ShouldBe(4);
        result.Stats.Health.ShouldBe(12);
        result.Stats.MinimumDamage.ShouldBe(3);
        result.Stats.MaximumDamage.ShouldBe(5);
        result.Stats.Initiative.ShouldBe(10.5);
        result.Stats.Speed.ShouldBe(6);
        result.RangedAttack.Shots.ShouldBe(12);
        result.RangedAttack.RangedAttackRange.ShouldBe(8);
        result.Morale.Value.ShouldBe(2);
        result.Luck.Value.ShouldBe(1);
        result.FactionId.ShouldBe(faction.Id);
    }

    [Fact(DisplayName = "Unit should create melee details when ranged values are omitted")]
    public void Create_Should_ReturnMeleeUnit_When_RangedValuesAreOmitted()
    {
        var faction = UnitTestData.CreateFaction();

        var result = Unit.Create(
            name: UnitName.Create(value: "Swordsman").Value,
            description: UnitDescription.Create(value: "A disciplined melee unit.").Value,
            stats: UnitCombatStats.Create(
                attack: 6,
                defense: 7,
                health: 18,
                minimumDamage: 2,
                maximumDamage: 4,
                initiative: 8,
                speed: 5).Value,
            rangedAttack: UnitRangedAttack.Create(
                shots: null,
                rangedAttackRange: null).Value,
            morale: UnitMorale.Create(value: 1).Value,
            luck: UnitLuck.Create(value: 0).Value,
            factionId: faction.Id);

        result.RangedAttack.Shots.ShouldBeNull();
        result.RangedAttack.RangedAttackRange.ShouldBeNull();
    }

    [Fact(DisplayName = "Unit should update all details when values are valid")]
    public void Update_Should_ReplaceDetails_When_ValuesAreValid()
    {
        var faction = UnitTestData.CreateFaction();
        var unit = UnitTestData.CreateUnit(faction);

        unit.Update(
            name: UnitName.Create(value: "Marksman").Value,
            description: UnitDescription.Create(value: "An elite ranged unit.").Value,
            stats: UnitCombatStats.Create(
                attack: 10,
                defense: 5,
                health: 14,
                minimumDamage: 4,
                maximumDamage: 7,
                initiative: 11.5,
                speed: 7).Value,
            rangedAttack: UnitRangedAttack.Create(
                shots: 16,
                rangedAttackRange: 10).Value,
            morale: UnitMorale.Create(value: 3).Value,
            luck: UnitLuck.Create(value: 2).Value,
            factionId: faction.Id);

        unit.Name.Value.ShouldBe("Marksman");
        unit.Stats.Attack.ShouldBe(10);
        unit.Stats.MaximumDamage.ShouldBe(7);
        unit.RangedAttack.Shots.ShouldBe(16);
        unit.Morale.Value.ShouldBe(3);
        unit.Luck.Value.ShouldBe(2);
    }

}
