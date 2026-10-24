using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Battle.UnitTests.Domain.Battles.Entities.Units.ValueObjects;

public sealed class DamageRollTests
{
    [Theory(DisplayName = "DamageRoll should preserve its value when value is non-negative")]
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
        DamageRoll.Create(-1).IsFailure.ShouldBeTrue();
    }
}
