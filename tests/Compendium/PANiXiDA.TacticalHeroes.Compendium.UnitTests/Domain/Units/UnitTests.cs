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
            stats: UnitCombatStats.Create(new UnitCombatStatsInput
            {
                Attack = 8,
                Defense = 4,
                Health = 12,
                MinimumDamage = 3,
                MaximumDamage = 5,
                Initiative = 10.5,
                Speed = 6,
                Shots = 12,
                RangedAttackRange = 8
            }).Value,
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
        result.Stats.Shots.ShouldBe(12);
        result.Stats.RangedAttackRange.ShouldBe(8);
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
            stats: UnitCombatStats.Create(new UnitCombatStatsInput
            {
                Attack = 6,
                Defense = 7,
                Health = 18,
                MinimumDamage = 2,
                MaximumDamage = 4,
                Initiative = 8,
                Speed = 5,
                Shots = null,
                RangedAttackRange = null
            }).Value,
            morale: UnitMorale.Create(value: 1).Value,
            luck: UnitLuck.Create(value: 0).Value,
            factionId: faction.Id);

        result.Stats.Shots.ShouldBeNull();
        result.Stats.RangedAttackRange.ShouldBeNull();
    }

    [Fact(DisplayName = "Unit should update all details when values are valid")]
    public void Update_Should_ReplaceDetails_When_ValuesAreValid()
    {
        var faction = UnitTestData.CreateFaction();
        var unit = UnitTestData.CreateUnit(faction);

        unit.Update(
            name: UnitName.Create(value: "Marksman").Value,
            description: UnitDescription.Create(value: "An elite ranged unit.").Value,
            stats: UnitCombatStats.Create(new UnitCombatStatsInput
            {
                Attack = 10,
                Defense = 5,
                Health = 14,
                MinimumDamage = 4,
                MaximumDamage = 7,
                Initiative = 11.5,
                Speed = 7,
                Shots = 16,
                RangedAttackRange = 10
            }).Value,
            morale: UnitMorale.Create(value: 3).Value,
            luck: UnitLuck.Create(value: 2).Value,
            factionId: faction.Id);

        unit.Name.Value.ShouldBe("Marksman");
        unit.Stats.Attack.ShouldBe(10);
        unit.Stats.MaximumDamage.ShouldBe(7);
        unit.Stats.Shots.ShouldBe(16);
        unit.Morale.Value.ShouldBe(3);
        unit.Luck.Value.ShouldBe(2);
    }

}
