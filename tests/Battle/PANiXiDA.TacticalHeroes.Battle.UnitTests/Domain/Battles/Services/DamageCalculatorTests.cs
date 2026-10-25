using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units.ValueObjects;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Services;

namespace PANiXiDA.TacticalHeroes.Battle.UnitTests.Domain.Battles.Services;

public sealed class DamageCalculatorTests
{
    [Theory(DisplayName = "Damage should apply the stat coefficient when attack and defense vary")]
    [InlineData(5, 5, 3, 2, 6)]
    [InlineData(15, 5, 3, 1, 5)]
    [InlineData(5, 15, 3, 1, 2)]
    [InlineData(0, 100, 1, 1, 0)]
    [InlineData(0, 0, 3, 0, 0)]
    public void Calculate_Should_ApplyStatCoefficient_When_AttackAndDefenseVary(
        int attack, int defense, int count, int roll, long expected)
    {
        var result = DamageCalculator.Calculate(
            UnitCombatStats.Create(attack, 0, 10).Value,
            UnitStackState.Create(count, 10).Value,
            DamageRange.Create(0, 2).Value,
            UnitCombatStats.Create(0, defense, 10).Value,
            roll);

        result.Value.Value.ShouldBe(expected);
    }

    [Fact(DisplayName = "Damage should round once after multiplication when total damage is fractional")]
    public void Calculate_Should_RoundOnceAfterMultiplication_When_TotalDamageIsFractional()
    {
        var result = DamageCalculator.Calculate(
            UnitCombatStats.Create(1, 0, 10).Value,
            UnitStackState.Create(10, 10).Value,
            DamageRange.Create(1, 1).Value,
            UnitCombatStats.Create(0, 0, 10).Value,
            1);

        result.Value.Value.ShouldBe(11);
    }

    [Theory(DisplayName = "Damage should reject the calculation when roll is outside range")]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(4)]
    public void Calculate_Should_ReturnFailure_When_RollIsOutsideRange(int roll)
    {
        var result = DamageCalculator.Calculate(
            UnitCombatStats.Create(0, 0, 10).Value,
            UnitStackState.Create(1, 10).Value,
            DamageRange.Create(1, 3).Value,
            UnitCombatStats.Create(0, 0, 10).Value,
            roll);

        result.IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Damage should reject the calculation when attacker is destroyed")]
    public void Calculate_Should_ReturnFailure_When_AttackerIsDestroyed()
    {
        var stats = UnitCombatStats.Create(0, 0, 10).Value;

        var result = DamageCalculator.Calculate(
            stats,
            UnitStackState.Create(0, 0).Value,
            DamageRange.Create(1, 1).Value,
            stats,
            1);

        result.IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Damage should preserve large totals when damage exceeds integer capacity")]
    public void Calculate_Should_PreserveLargeTotals_When_DamageExceedsIntegerCapacity()
    {
        var stats = UnitCombatStats.Create(0, 0, int.MaxValue).Value;

        var result = DamageCalculator.Calculate(
            stats,
            UnitStackState.Create(int.MaxValue, int.MaxValue).Value,
            DamageRange.Create(int.MaxValue, int.MaxValue).Value,
            stats,
            int.MaxValue);

        result.Value.Value.ShouldBe((long)int.MaxValue * int.MaxValue);
    }

    [Fact(DisplayName = "Damage should reject unsupported totals when damage exceeds long capacity")]
    public void Calculate_Should_ReturnFailure_When_DamageExceedsLongCapacity()
    {
        var result = DamageCalculator.Calculate(
            UnitCombatStats.Create(int.MaxValue, 0, 10).Value,
            UnitStackState.Create(int.MaxValue, 10).Value,
            DamageRange.Create(int.MaxValue, int.MaxValue).Value,
            UnitCombatStats.Create(0, 0, 10).Value,
            int.MaxValue);

        result.IsFailure.ShouldBeTrue();
    }
}
