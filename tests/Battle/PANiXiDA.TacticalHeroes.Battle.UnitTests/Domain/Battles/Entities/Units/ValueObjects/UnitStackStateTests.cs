using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Battle.UnitTests.Domain.Battles.Entities.Units.ValueObjects;

public sealed class UnitStackStateTests
{
    [Theory(DisplayName = "Stack state should reject inconsistent count and health when values are invalid")]
    [InlineData(-1, 1)]
    [InlineData(1, -1)]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    public void Create_Should_ReturnFailure_When_ValuesAreInvalid(int count, int health)
    {
        UnitStackState.Create(count, health).IsFailure.ShouldBeTrue();
    }

    [Theory(DisplayName = "Stack state should preserve living or destroyed state when values are valid")]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(int.MaxValue, int.MaxValue)]
    public void Create_Should_PreserveState_When_ValuesAreValid(int count, int health)
    {
        var result = UnitStackState.Create(count, health);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(count);
        result.Value.CurrentUnitHealth.ShouldBe(health);
    }
}
