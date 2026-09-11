using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Domain.Heroes.ValueObjects;

public sealed class HeroCombatStatsTests
{
    [Fact(DisplayName = "Hero combat stats should create values when values are valid")]
    public void Create_Should_ReturnStats_When_ValuesAreValid()
    {
        var result = HeroCombatStats.Create(
            attack: 8,
            defense: 6,
            minimumDamage: 3,
            maximumDamage: 7,
            initiative: 10.5);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Attack.ShouldBe(8);
        result.Value.Defense.ShouldBe(6);
        result.Value.MinimumDamage.ShouldBe(3);
        result.Value.MaximumDamage.ShouldBe(7);
        result.Value.Initiative.ShouldBe(10.5);
    }

    [Fact(DisplayName = "Hero combat stats should reject values when values are invalid")]
    public void Create_Should_ReturnValidationFailure_When_ValuesAreInvalid()
    {
        var result = HeroCombatStats.Create(
            attack: -1,
            defense: -1,
            minimumDamage: 8,
            maximumDamage: 7,
            initiative: double.PositiveInfinity);

        result.IsFailure.ShouldBeTrue();
        var fields = result.Errors
            .Select(error => error.Metadata.GetValueOrDefault(Error.FieldMetadataKey))
            .ToArray();
        fields.ShouldContain(nameof(HeroCombatStats.Attack));
        fields.ShouldContain(nameof(HeroCombatStats.Defense));
        fields.ShouldContain(nameof(HeroCombatStats.MaximumDamage));
        fields.ShouldContain(nameof(HeroCombatStats.Initiative));
    }
}
