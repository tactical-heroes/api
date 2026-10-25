using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Battle.UnitTests.Domain.Battles.ValueObjects;

public sealed class DamageTests
{
    [Fact(DisplayName = "Domain value should describe its state when converted to text")]
    public void ToString_Should_DescribeState_When_ConvertedToText()
    {
        var value = Damage.Create(543).Value;

        var text = value.ToString();

        text.ShouldBe("Damage { Value = 543 }");
    }

    [Theory(DisplayName = "Damage should preserve its value when value is non negative")]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(long.MaxValue)]
    public void Create_Should_PreserveValue_When_ValueIsNonNegative(long value)
    {
        var result = Damage.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(value);
    }

    [Fact(DisplayName = "Damage should reject its value when value is negative")]
    public void Create_Should_ReturnFailure_When_ValueIsNegative()
    {
        var result = Damage.Create(-1);

        result.IsFailure.ShouldBeTrue();
    }
}
