using PANiXiDA.TacticalHeroes.Compendium.Domain.Units.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Domain.Units.ValueObjects;

public sealed class UnitCombatStatsTests
{
    [Fact(DisplayName = "Unit combat stats should create ranged values when values are valid")]
    public void Create_Should_ReturnStats_When_ValuesAreValid()
    {
        var result = UnitCombatStats.Create(new UnitCombatStatsInput
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
        });

        result.IsSuccess.ShouldBeTrue();
        result.Value.Attack.ShouldBe(8);
        result.Value.Defense.ShouldBe(4);
        result.Value.Health.ShouldBe(12);
        result.Value.MinimumDamage.ShouldBe(3);
        result.Value.MaximumDamage.ShouldBe(5);
        result.Value.Initiative.ShouldBe(10.5);
        result.Value.Speed.ShouldBe(6);
        result.Value.Shots.ShouldBe(12);
        result.Value.RangedAttackRange.ShouldBe(8);
    }

    [Fact(DisplayName = "Unit combat stats should reject values when values are invalid")]
    public void Create_Should_ReturnValidationFailure_When_ValuesAreInvalid()
    {
        var result = UnitCombatStats.Create(new UnitCombatStatsInput
        {
            Attack = -1,
            Defense = -1,
            Health = 0,
            MinimumDamage = 6,
            MaximumDamage = 5,
            Initiative = double.PositiveInfinity,
            Speed = -1,
            Shots = null,
            RangedAttackRange = 0
        });

        result.IsFailure.ShouldBeTrue();
        var fields = result.Errors
            .Select(error => error.Metadata.GetValueOrDefault(Error.FieldMetadataKey))
            .ToArray();
        fields.ShouldContain(nameof(UnitCombatStats.Attack));
        fields.ShouldContain(nameof(UnitCombatStats.Defense));
        fields.ShouldContain(nameof(UnitCombatStats.Health));
        fields.ShouldContain(nameof(UnitCombatStats.MaximumDamage));
        fields.ShouldContain(nameof(UnitCombatStats.Initiative));
        fields.ShouldContain(nameof(UnitCombatStats.Speed));
        fields.ShouldContain(nameof(UnitCombatStats.RangedAttackRange));
    }
}
