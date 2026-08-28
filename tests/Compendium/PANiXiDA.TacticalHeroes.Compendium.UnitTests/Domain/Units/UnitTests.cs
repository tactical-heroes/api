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

        var result = Unit.Create(new UnitAttributes
        {
            Name = "  Archer  ",
            Description = "  A disciplined ranged unit.  ",
            CombatStats = new UnitCombatStatsInput
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
            },
            Morale = 2,
            Luck = 1,
            FactionId = faction.Id.Value
        });

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

        var result = Unit.Create(new UnitAttributes
        {
            Name = "Swordsman",
            Description = "A disciplined melee unit.",
            CombatStats = new UnitCombatStatsInput
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
            },
            Morale = 1,
            Luck = 0,
            FactionId = faction.Id.Value
        });

        result.IsSuccess.ShouldBeTrue();
        result.Value.Stats.Shots.ShouldBeNull();
        result.Value.Stats.RangedAttackRange.ShouldBeNull();
    }

    [Fact(DisplayName = "Unit should reject statistics when statistics are invalid")]
    public void Create_Should_ReturnValidationFailure_When_StatisticsAreInvalid()
    {
        var faction = UnitTestData.CreateFaction();

        var result = Unit.Create(new UnitAttributes
        {
            Name = "Archer",
            Description = "A disciplined ranged unit.",
            CombatStats = new UnitCombatStatsInput
            {
                Attack = -1,
                Defense = -1,
                Health = 0,
                MinimumDamage = 6,
                MaximumDamage = 5,
                Initiative = double.NaN,
                Speed = -1,
                Shots = 12,
                RangedAttackRange = null
            },
            Morale = 6,
            Luck = -1,
            FactionId = faction.Id.Value
        });

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

        var result = unit.Update(new UnitAttributes
        {
            Name = "Marksman",
            Description = "An elite ranged unit.",
            CombatStats = new UnitCombatStatsInput
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
            },
            Morale = 3,
            Luck = 2,
            FactionId = faction.Id.Value
        });

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

        var result = unit.Update(new UnitAttributes
        {
            Name = "Marksman",
            Description = "An elite ranged unit.",
            CombatStats = new UnitCombatStatsInput
            {
                Attack = -1,
                Defense = 5,
                Health = 14,
                MinimumDamage = 4,
                MaximumDamage = 7,
                Initiative = 11.5,
                Speed = 7,
                Shots = 16,
                RangedAttackRange = 10
            },
            Morale = 3,
            Luck = 2,
            FactionId = faction.Id.Value
        });

        result.IsFailure.ShouldBeTrue();
        unit.Name.Value.ShouldBe("Archer");
        unit.Stats.Attack.ShouldBe(8);
        unit.Stats.MaximumDamage.ShouldBe(5);
        unit.Morale.Value.ShouldBe(2);
    }
}
