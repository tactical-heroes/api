using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Battle.UnitTests.Domain.Battles.ValueObjects;

public sealed class DamageTests
{
    [Theory(DisplayName = "Damage should preserve its value when value is non-negative")]
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
        Damage.Create(-1).IsFailure.ShouldBeTrue();
    }
}
