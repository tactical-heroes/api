using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Services;

namespace PANiXiDA.TacticalHeroes.Battle.UnitTests.Domain.Battles.Services;

public sealed class DamageRandomizerTests
{
    [Theory(DisplayName = "Damage randomizer should return the bound when both bounds are equal")]
    [InlineData(0)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void Roll_Should_ReturnBound_When_BothBoundsAreEqual(int value)
    {
        var result = DamageRandomizer.Roll(value, value);

        result.ShouldBe(value);
    }

    [Theory(DisplayName = "Damage randomizer should keep rolls within bounds when bounds are ordered")]
    [InlineData(0, 1)]
    [InlineData(3, 5)]
    [InlineData(-5, -1)]
    [InlineData(0, int.MaxValue)]
    [InlineData(int.MinValue, int.MaxValue)]
    public void Roll_Should_KeepRollsWithinBounds_When_BoundsAreOrdered(int minimum, int maximum)
    {
        var results = Enumerable.Range(0, 64)
            .Select(_ => DamageRandomizer.Roll(minimum, maximum))
            .ToArray();

        results.ShouldAllBe(value => value >= minimum && value <= maximum);
    }
}
