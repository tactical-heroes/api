using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Battle.UnitTests.Domain.Battles.Entities.Units.ValueObjects;

public sealed class DamageRollTests
{
    [Fact(DisplayName = "Domain value should describe its state when converted to text")]
    public void ToString_Should_DescribeState_When_ConvertedToText()
    {
        var value = DamageRoll.Create(3).Value;

        var text = value.ToString();

        text.ShouldBe("DamageRoll { Value = 3 }");
    }

    [Theory(DisplayName = "DamageRoll should preserve its value when value is non negative")]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(int.MaxValue)]
    public void Create_Should_PreserveValue_When_ValueIsNonNegative(int value)
    {
        var result = DamageRoll.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(value);
    }

    [Fact(DisplayName = "DamageRoll should reject its value when value is negative")]
    public void Create_Should_ReturnFailure_When_ValueIsNegative()
    {
        var result = DamageRoll.Create(-1);

        result.IsFailure.ShouldBeTrue();
    }
}
