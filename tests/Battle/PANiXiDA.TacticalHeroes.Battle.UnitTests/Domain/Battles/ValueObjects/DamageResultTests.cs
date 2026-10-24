using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units.ValueObjects;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Battle.UnitTests.Domain.Battles.ValueObjects;

public sealed class DamageResultTests
{
    [Fact(DisplayName = "Domain value should describe its state when converted to text")]
    public void ToString_Should_DescribeState_When_ConvertedToText()
    {
        var value = DamageResult.Create(
            Damage.Create(1000).Value, Damage.Create(24).Value, 3, UnitStackState.Create(0, 0).Value).Value;

        var text = value.ToString();

        text.ShouldContain("CalculatedDamage = Damage { Value = 1000 }");
        text.ShouldContain("AppliedDamage = Damage { Value = 24 }");
        text.ShouldContain("KilledCount = 3");
        text.ShouldContain("RemainingStack = UnitStackState { Count = 0, CurrentUnitHealth = 0 }");
    }

    [Theory(DisplayName = "Damage result should reject its values when losses are invalid")]
    [InlineData(2, 3, 0)]
    [InlineData(2, 1, -1)]
    public void Create_Should_ReturnFailure_When_LossesAreInvalid(long calculated, long applied, int killed)
    {
        var result = DamageResult.Create(
            Damage.Create(calculated).Value,
            Damage.Create(applied).Value,
            killed,
            UnitStackState.Create(1, 1).Value);

        result.IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Damage result should distinguish overkill when calculated damage exceeds remaining health")]
    public void Create_Should_PreserveOverkill_When_CalculatedDamageExceedsRemainingHealth()
    {
        var result = DamageResult.Create(
            Damage.Create(1000).Value,
            Damage.Create(24).Value,
            3,
            UnitStackState.Create(0, 0).Value).Value;

        result.CalculatedDamage.Value.ShouldBe(1000);
        result.AppliedDamage.Value.ShouldBe(24);
        result.KilledCount.ShouldBe(3);
        result.RemainingStack.Count.ShouldBe(0);
    }
}
