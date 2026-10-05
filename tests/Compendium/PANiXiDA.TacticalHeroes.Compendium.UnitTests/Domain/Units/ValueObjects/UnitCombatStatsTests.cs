using PANiXiDA.TacticalHeroes.Compendium.Domain.Units.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Domain.Units.ValueObjects;

public sealed class UnitCombatStatsTests
{
    [Fact(DisplayName = "Unit combat stats should format all components when converted to string")]
    public void ToString_Should_FormatAllComponents_When_ConvertedToString()
    {
        var stats = UnitCombatStats.Create(8, 4, 12, 3, 5, 10.5, 6).Value;

        var result = stats.ToString();

        result.ShouldBe("UnitCombatStats { Attack = 8, Defense = 4, Health = 12, MinimumDamage = 3, MaximumDamage = 5, Initiative = 10.5, Speed = 6 }");
    }

    [Fact(DisplayName = "Unit combat stats should create values when values are valid")]
    public void Create_Should_ReturnStats_When_ValuesAreValid()
    {
        var result = UnitCombatStats.Create(
            attack: 8,
            defense: 4,
            health: 12,
            minimumDamage: 3,
            maximumDamage: 5,
            initiative: 10.5,
            speed: 6);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Attack.ShouldBe(8);
        result.Value.Defense.ShouldBe(4);
        result.Value.Health.ShouldBe(12);
        result.Value.MinimumDamage.ShouldBe(3);
        result.Value.MaximumDamage.ShouldBe(5);
        result.Value.Initiative.ShouldBe(10.5);
        result.Value.Speed.ShouldBe(6);
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
            speed: -1);

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
    }
}
