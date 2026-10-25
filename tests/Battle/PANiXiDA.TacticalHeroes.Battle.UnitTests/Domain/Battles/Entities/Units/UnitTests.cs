using PANiXiDA.TacticalHeroes.Battle.Domain.Battles;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units.ValueObjects;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.ValueObjects;
using PANiXiDA.TacticalHeroes.Battle.UnitTests.Battles;

namespace PANiXiDA.TacticalHeroes.Battle.UnitTests.Domain.Battles.Entities.Units;

public sealed class UnitTests
{
    [Fact(DisplayName = "Unit should preserve wounds when a wounded stack is supplied")]
    public void Create_Should_PreserveWounds_When_AWoundedStackIsSupplied()
    {
        var stats = UnitCombatStats.Create(0, 0, 10).Value;
        var damageRange = DamageRange.Create(1, 1).Value;
        var stackState = UnitStackState.Create(3, 4).Value;

        var unit = Unit.Create(stats, damageRange, stackState).Value;

        unit.Id.Value.ShouldNotBe(Guid.Empty);
        unit.Count.ShouldBe(3);
        unit.CurrentUnitHealth.ShouldBe(4);
        unit.TotalHealth.ShouldBe(24);
    }

    [Fact(DisplayName = "Unit should reject excessive health when current health exceeds maximum")]
    public void Create_Should_ReturnFailure_When_CurrentHealthExceedsMaximum()
    {
        var result = Unit.Create(
            UnitCombatStats.Create(0, 0, 10).Value,
            DamageRange.Create(1, 1).Value,
            UnitStackState.Create(1, 11).Value);

        result.IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Unit should support large health totals when stack exceeds integer capacity")]
    public void Create_Should_SupportLargeTotals_When_StackExceedsIntegerCapacity()
    {
        var unit = BattleTestData.CreateUnit(maxHealth: int.MaxValue, count: int.MaxValue);

        unit.TotalHealth.ShouldBe((long)int.MaxValue * int.MaxValue);
        unit.IsAlive.ShouldBeTrue();
        unit.BattleId.ShouldBeNull();
    }

    [Theory(DisplayName = "Damage should preserve stack invariants when a wounded stack is hit")]
    [InlineData(0, 3, 4, 0, 0)]
    [InlineData(2, 3, 2, 0, 2)]
    [InlineData(4, 2, 10, 1, 4)]
    [InlineData(7, 2, 7, 1, 7)]
    [InlineData(14, 1, 10, 2, 14)]
    [InlineData(23, 1, 1, 2, 23)]
    [InlineData(24, 0, 0, 3, 24)]
    [InlineData(1000, 0, 0, 3, 24)]
    public void TakeDamage_Should_PreserveStackInvariants_When_AWoundedStackIsHit(
        long damage, int count, int health, int killed, long applied)
    {
        var unit = BattleTestData.CreateUnit(currentHealth: 4);

        var result = unit.TakeDamage(Damage.Create(damage).Value).Value;

        unit.Count.ShouldBe(count);
        unit.CurrentUnitHealth.ShouldBe(health);
        result.KilledCount.ShouldBe(killed);
        result.AppliedDamage.Value.ShouldBe(applied);
        result.CalculatedDamage.Value.ShouldBe(damage);
        result.RemainingStack.ShouldBe(unit.StackState);
        unit.IsAlive.ShouldBe(count > 0);
    }

    [Fact(DisplayName = "Damage should accumulate wounds when several small hits occur")]
    public void TakeDamage_Should_AccumulateWounds_When_SeveralSmallHitsOccur()
    {
        var unit = BattleTestData.CreateUnit(currentHealth: 4);

        var firstHit = unit.TakeDamage(Damage.Create(2).Value).Value;
        var secondHit = unit.TakeDamage(Damage.Create(2).Value).Value;

        firstHit.RemainingStack.CurrentUnitHealth.ShouldBe(2);
        secondHit.KilledCount.ShouldBe(1);
        unit.Count.ShouldBe(2);
        unit.CurrentUnitHealth.ShouldBe(10);
    }

    [Fact(DisplayName = "Damage should cap casualties when damage exceeds a large stack health")]
    public void TakeDamage_Should_CapCasualties_When_DamageExceedsALargeStackHealth()
    {
        var unit = BattleTestData.CreateUnit(maxHealth: int.MaxValue, count: int.MaxValue);

        var result = unit.TakeDamage(Damage.Create(long.MaxValue).Value).Value;

        result.AppliedDamage.Value.ShouldBe((long)int.MaxValue * int.MaxValue);
        result.KilledCount.ShouldBe(int.MaxValue);
        unit.TotalHealth.ShouldBe(0);
    }

    [Fact(DisplayName = "Damage should reject a target when target is destroyed")]
    public void TakeDamage_Should_ReturnFailure_When_TargetIsDestroyed()
    {
        var unit = BattleTestData.CreateUnit(count: 0);

        var result = unit.TakeDamage(Damage.Create(1).Value);

        result.IsFailure.ShouldBeTrue();
        unit.TotalHealth.ShouldBe(0);
    }

    [Fact(DisplayName = "Unit should retain its owner when assigned to another battle")]
    public void AssignToBattle_Should_RejectReassignment_When_AssignedToAnotherBattle()
    {
        var unit = BattleTestData.CreateUnit();
        var owner = BattleId.New();
        unit.AssignToBattle(owner);

        Should.Throw<InvalidOperationException>(() => unit.AssignToBattle(BattleId.New()));

        unit.BattleId.ShouldBe(owner);
    }

    [Fact(DisplayName = "Unit should reject its owner when battle id is empty")]
    public void AssignToBattle_Should_RejectOwner_When_BattleIdIsEmpty()
    {
        var unit = BattleTestData.CreateUnit();

        Should.Throw<ArgumentException>(() => unit.AssignToBattle(default));

        unit.BattleId.ShouldBeNull();
    }
}
