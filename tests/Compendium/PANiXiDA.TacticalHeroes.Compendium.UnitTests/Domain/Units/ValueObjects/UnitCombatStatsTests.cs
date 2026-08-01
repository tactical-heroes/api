using PANiXiDA.TacticalHeroes.Compendium.Domain.Units.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Domain.Units.ValueObjects;

public sealed class UnitCombatStatsTests
{
    [Fact(DisplayName = "Unit combat stats should create ranged values when values are valid")]
    public void Create_Should_ReturnStats_When_ValuesAreValid()
    {
        var result = UnitCombatStats.Create(
            attack: 8,
            defense: 4,
            health: 12,
            minimumDamage: 3,
            maximumDamage: 5,
            initiative: 10.5,
            speed: 6,
            shots: 12,
            rangedAttackRange: 8);

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
        var result = UnitCombatStats.Create(
            attack: -1,
            defense: -1,
            health: 0,
            minimumDamage: 6,
            maximumDamage: 5,
            initiative: double.PositiveInfinity,
            speed: -1,
            shots: null,
            rangedAttackRange: 0);

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
