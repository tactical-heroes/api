using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Abstractions;
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

    [Fact(DisplayName = "Damage roll should include the largest integer when random source returns upper bound")]
    public void Roll_Should_SupportMaximumInteger_When_RandomSourceReturnsUpperBound()
    {
        var range = DamageRange.Create(0, int.MaxValue).Value;
        var random = Substitute.For<IRandomSource>();
        random.NextInt64(0, (long)int.MaxValue + 1).Returns(int.MaxValue);

        var result = range.Roll(random);

        result.Value.Value.ShouldBe(int.MaxValue);
    }

    [Theory(DisplayName = "Damage roll should reject the random output when random source violates bounds")]
    [InlineData(-1)]
    [InlineData(4)]
    public void Roll_Should_ReturnFailure_When_RandomSourceViolatesBounds(long value)
    {
        var range = DamageRange.Create(0, 3).Value;
        var random = Substitute.For<IRandomSource>();
        random.NextInt64(0, 4).Returns(value);

        range.Roll(random).IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Damage roll should avoid consuming randomness when damage is fixed")]
    public void Roll_Should_NotConsumeRandomness_When_DamageIsFixed()
    {
        var range = DamageRange.Create(2, 2).Value;
        var random = Substitute.For<IRandomSource>();

        var result = range.Roll(random);

        result.Value.Value.ShouldBe(2);
        random.DidNotReceiveWithAnyArgs().NextInt64(default, default);
    }
}
