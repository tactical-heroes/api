using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Battle.UnitTests.Domain.Battles.Entities.Units.ValueObjects;

public sealed class UnitCombatStatsTests
{
    [Theory(DisplayName = "Unit stats should reject invalid characteristics when values are invalid")]
    [InlineData(-1, 0, 10)]
    [InlineData(0, -1, 10)]
    [InlineData(0, 0, 0)]
    [InlineData(0, 0, -1)]
    public void Create_Should_ReturnFailure_When_ValuesAreInvalid(int attack, int defense, int health)
    {
        UnitCombatStats.Create(attack, defense, health).IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Unit stats should support large characteristics when values are valid")]
    public void Create_Should_SupportLargeCharacteristics_When_ValuesAreValid()
    {
        var result = UnitCombatStats.Create(int.MaxValue, int.MaxValue, int.MaxValue);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Attack.ShouldBe(int.MaxValue);
        result.Value.Defense.ShouldBe(int.MaxValue);
        result.Value.MaxHealth.ShouldBe(int.MaxValue);
    }
}
