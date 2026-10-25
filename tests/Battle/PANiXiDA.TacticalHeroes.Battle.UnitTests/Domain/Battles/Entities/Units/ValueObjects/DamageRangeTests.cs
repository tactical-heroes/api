using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Battle.UnitTests.Domain.Battles.Entities.Units.ValueObjects;

public sealed class DamageRangeTests
{
    [Fact(DisplayName = "Domain value should describe its state when converted to text")]
    public void ToString_Should_DescribeState_When_ConvertedToText()
    {
        var value = DamageRange.Create(2, 4).Value;

        var text = value.ToString();

        text.ShouldBe("DamageRange { Minimum = 2, Maximum = 4 }");
    }

    [Theory(DisplayName = "Damage range should reject its bounds when bounds are invalid")]
    [InlineData(-1, 1)]
    [InlineData(2, 1)]
    public void Create_Should_ReturnFailure_When_BoundsAreInvalid(int minimum, int maximum)
    {
        var result = DamageRange.Create(minimum, maximum);

        result.IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Damage range should preserve inclusive bounds when bounds are valid")]
    public void Create_Should_PreserveBounds_When_BoundsAreValid()
    {
        var range = DamageRange.Create(0, int.MaxValue).Value;

        range.Minimum.ShouldBe(0);
        range.Maximum.ShouldBe(int.MaxValue);
    }

    [Theory(DisplayName = "Damage range should preserve the value when damage is fixed")]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(int.MaxValue)]
    public void Roll_Should_PreserveValue_When_DamageIsFixed(int value)
    {
        var range = DamageRange.Create(value, value).Value;

        var result = range.Roll();

        result.ShouldBe(value);
    }

    [Theory(DisplayName = "Damage range should keep rolls within bounds when damage varies")]
    [InlineData(0, 1)]
    [InlineData(3, 5)]
    [InlineData(0, int.MaxValue)]
    public void Roll_Should_KeepRollsWithinBounds_When_DamageVaries(int minimum, int maximum)
    {
        var range = DamageRange.Create(minimum, maximum).Value;

        var results = Enumerable.Range(0, 64).Select(_ => range.Roll()).ToArray();

        results.ShouldAllBe(value => value >= minimum && value <= maximum);
    }
}
