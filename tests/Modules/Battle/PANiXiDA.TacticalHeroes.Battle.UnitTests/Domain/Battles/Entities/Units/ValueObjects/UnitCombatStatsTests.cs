using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Battle.UnitTests.Domain.Battles.Entities.Units.ValueObjects;

public sealed class UnitCombatStatsTests
{
    [Fact(DisplayName = "Domain value should describe its state when converted to text")]
    public void ToString_Should_DescribeState_When_ConvertedToText()
    {
        var value = UnitCombatStats.Create(2, 3, 10).Value;

        var text = value.ToString();

        text.ShouldBe("UnitCombatStats { Attack = 2, Defense = 3, MaxHealth = 10 }");
    }

    [Theory(DisplayName = "Unit stats should reject invalid characteristics when values are invalid")]
    [InlineData(-1, 0, 10)]
    [InlineData(0, -1, 10)]
    [InlineData(0, 0, 0)]
    [InlineData(0, 0, -1)]
    public void Create_Should_ReturnFailure_When_ValuesAreInvalid(int attack, int defense, int health)
    {
        var result = UnitCombatStats.Create(attack, defense, health);

        result.IsFailure.ShouldBeTrue();
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
