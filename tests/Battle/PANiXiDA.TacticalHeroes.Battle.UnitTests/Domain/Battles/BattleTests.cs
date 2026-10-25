using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Events;
using PANiXiDA.TacticalHeroes.Battle.UnitTests.Battles;

using BattleAggregate = PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Battle;

namespace PANiXiDA.TacticalHeroes.Battle.UnitTests.Domain.Battles;

public sealed class BattleTests
{
    [Fact(DisplayName = "Battle should generate its identity when a battle is created")]
    public void Create_Should_GenerateIdentity_When_ABattleIsCreated()
    {
        Unit[] units = [BattleTestData.CreateUnit(), BattleTestData.CreateUnit()];

        var battle = BattleAggregate.Create(units).Value;

        battle.Id.Value.ShouldNotBe(Guid.Empty);
        battle.Units.ShouldAllBe(unit => unit.BattleId == battle.Id);
    }

    [Fact(DisplayName = "Battle should assign its units when participants are distinct")]
    public void Create_Should_AssignUnits_When_ParticipantsAreDistinct()
    {
        var first = BattleTestData.CreateUnit();
        var second = BattleTestData.CreateUnit();
        var input = new List<Unit> { first, second };

        var battle = BattleAggregate.Create(input).Value;
        input.Clear();

        battle.Units.Count.ShouldBe(2);
        first.BattleId.ShouldBe(battle.Id);
        second.BattleId.ShouldBe(battle.Id);
        Should.Throw<NotSupportedException>(() => ((ICollection<Unit>)battle.Units).Clear());
    }

    [Fact(DisplayName = "Battle should reject participants when participants are duplicated")]
    public void Create_Should_ReturnFailure_When_ParticipantsAreDuplicated()
    {
        var unit = BattleTestData.CreateUnit();

        var result = BattleAggregate.Create([unit, unit]);

        result.IsFailure.ShouldBeTrue();
        unit.BattleId.ShouldBeNull();
    }

    [Fact(DisplayName = "Battle should preserve participants when a participant is null")]
    public void Create_Should_PreserveParticipants_When_AParticipantIsNull()
    {
        var unit = BattleTestData.CreateUnit();

        var result = BattleAggregate.Create([unit, null!]);

        result.IsFailure.ShouldBeTrue();
        unit.BattleId.ShouldBeNull();
    }

    [Theory(DisplayName = "Battle should reject participants when participant count is insufficient")]
    [InlineData(0)]
    [InlineData(1)]
    public void Create_Should_ReturnFailure_When_ParticipantCountIsInsufficient(int count)
    {
        var units = Enumerable.Range(0, count).Select(_ => BattleTestData.CreateUnit());

        BattleAggregate.Create(units).IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Battle should preserve ownership when a participant already belongs to another battle")]
    public void Create_Should_PreserveOwnership_When_AParticipantAlreadyBelongsToAnotherBattle()
    {
        var assigned = BattleTestData.CreateUnit();
        var other = BattleTestData.CreateUnit();
        var original = BattleAggregate.Create([assigned, other]).Value;
        var unassigned = BattleTestData.CreateUnit();

        var result = BattleAggregate.Create([unassigned, assigned]);

        result.IsFailure.ShouldBeTrue();
        unassigned.BattleId.ShouldBeNull();
        assigned.BattleId.ShouldBe(original.Id);
    }

    [Fact(DisplayName = "Attack should damage only the defender when attack is valid")]
    public void Attack_Should_DamageOnlyDefender_When_AttackIsValid()
    {
        var attacker = BattleTestData.CreateUnit(minimumDamage: 7, maximumDamage: 7, count: 1);
        var defender = BattleTestData.CreateUnit(currentHealth: 4);
        var battle = BattleAggregate.Create([attacker, defender]).Value;

        var result = battle.Attack(attacker.Id, defender.Id);

        result.IsSuccess.ShouldBeTrue();
        attacker.TotalHealth.ShouldBe(10);
        defender.Count.ShouldBe(2);
        defender.CurrentUnitHealth.ShouldBe(7);
        var domainEvent = battle.GetDomainEvents().ShouldHaveSingleItem().ShouldBeOfType<UnitDamaged>();
        domainEvent.BattleId.ShouldBe(battle.Id.Value);
        domainEvent.AttackerId.ShouldBe(attacker.Id.Value);
        domainEvent.DefenderId.ShouldBe(defender.Id.Value);
        domainEvent.CalculatedDamage.ShouldBe(7);
        domainEvent.AppliedDamage.ShouldBe(7);
        domainEvent.KilledCount.ShouldBe(1);
        domainEvent.RemainingCount.ShouldBe(2);
        domainEvent.CurrentUnitHealth.ShouldBe(7);
    }

    [Fact(DisplayName = "Attack should capture actual losses when damage exceeds remaining health")]
    public void Attack_Should_CaptureActualLosses_When_DamageExceedsRemainingHealth()
    {
        var attacker = BattleTestData.CreateUnit(minimumDamage: 7, maximumDamage: 7, count: 1);
        var defender = BattleTestData.CreateUnit(count: 1, currentHealth: 4);
        var battle = BattleAggregate.Create([attacker, defender]).Value;

        battle.Attack(attacker.Id, defender.Id);

        var domainEvent = battle.GetDomainEvents().ShouldHaveSingleItem().ShouldBeOfType<UnitDamaged>();
        domainEvent.CalculatedDamage.ShouldBe(7);
        domainEvent.AppliedDamage.ShouldBe(4);
        domainEvent.KilledCount.ShouldBe(1);
        domainEvent.RemainingCount.ShouldBe(0);
        domainEvent.CurrentUnitHealth.ShouldBe(0);
    }

    [Fact(DisplayName = "Attack should reject the attacker when attacker targets itself")]
    public void Attack_Should_ReturnFailure_When_AttackerTargetsItself()
    {
        var unit = BattleTestData.CreateUnit();
        var battle = BattleAggregate.Create([unit, BattleTestData.CreateUnit()]).Value;

        var result = battle.Attack(unit.Id, unit.Id);

        result.IsFailure.ShouldBeTrue();
        unit.TotalHealth.ShouldBe(30);
        battle.GetDomainEvents().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Attack should preserve units when participant belongs to another battle")]
    public void Attack_Should_PreserveUnits_When_ParticipantBelongsToAnotherBattle()
    {
        var attacker = BattleTestData.CreateUnit();
        var defender = BattleTestData.CreateUnit();
        var battle = BattleAggregate.Create([attacker, defender]).Value;
        var foreign = BattleTestData.CreateUnit();
        BattleAggregate.Create([foreign, BattleTestData.CreateUnit()]);

        var result = battle.Attack(attacker.Id, foreign.Id);

        result.IsFailure.ShouldBeTrue();
        defender.TotalHealth.ShouldBe(30);
        foreign.TotalHealth.ShouldBe(30);
        battle.GetDomainEvents().ShouldBeEmpty();
    }

    [Theory(DisplayName = "Attack should reject participants when a participant is destroyed")]
    [InlineData(true)]
    [InlineData(false)]
    public void Attack_Should_ReturnFailure_When_AParticipantIsDestroyed(bool destroyedAttacker)
    {
        var attacker = BattleTestData.CreateUnit(count: destroyedAttacker ? 0 : 1);
        var defender = BattleTestData.CreateUnit(count: destroyedAttacker ? 1 : 0);
        var battle = BattleAggregate.Create([attacker, defender]).Value;

        var result = battle.Attack(attacker.Id, defender.Id);

        result.IsFailure.ShouldBeTrue();
        battle.GetDomainEvents().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Attack should select damage within bounds when random damage is configured")]
    public void Attack_Should_SelectDamageWithinBounds_When_RandomDamageIsConfigured()
    {
        var attacker = BattleTestData.CreateUnit(minimumDamage: 2, maximumDamage: 4, count: 1);
        var defender = BattleTestData.CreateUnit();
        var battle = BattleAggregate.Create([attacker, defender]).Value;

        var result = battle.Attack(attacker.Id, defender.Id);

        result.IsSuccess.ShouldBeTrue();
        var domainEvent = battle.GetDomainEvents().ShouldHaveSingleItem().ShouldBeOfType<UnitDamaged>();
        domainEvent.CalculatedDamage.ShouldBeInRange(2L, 4L);
        domainEvent.AppliedDamage.ShouldBe(domainEvent.CalculatedDamage);
        defender.TotalHealth.ShouldBe(30 - domainEvent.AppliedDamage);
    }

    [Fact(DisplayName = "Attack should preserve the defender when calculated damage exceeds supported capacity")]
    public void Attack_Should_PreserveDefender_When_CalculatedDamageExceedsSupportedCapacity()
    {
        var attacker = BattleTestData.CreateUnit(
            attack: int.MaxValue,
            minimumDamage: int.MaxValue,
            maximumDamage: int.MaxValue,
            count: int.MaxValue);
        var defender = BattleTestData.CreateUnit();
        var battle = BattleAggregate.Create([attacker, defender]).Value;

        var result = battle.Attack(attacker.Id, defender.Id);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeEmpty();
        defender.Count.ShouldBe(3);
        defender.CurrentUnitHealth.ShouldBe(10);
        defender.TotalHealth.ShouldBe(30);
        battle.GetDomainEvents().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Attack should avoid damage events when calculated damage is zero")]
    public void Attack_Should_NotRaiseDamageEvent_When_CalculatedDamageIsZero()
    {
        var attacker = BattleTestData.CreateUnit(minimumDamage: 0, maximumDamage: 0);
        var defender = BattleTestData.CreateUnit();
        var battle = BattleAggregate.Create([attacker, defender]).Value;

        var result = battle.Attack(attacker.Id, defender.Id);

        result.IsSuccess.ShouldBeTrue();
        defender.TotalHealth.ShouldBe(30);
        battle.GetDomainEvents().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Attack should preserve past events when another hit changes the target")]
    public void Attack_Should_PreservePastEvents_When_AnotherHitChangesTheTarget()
    {
        var attacker = BattleTestData.CreateUnit(count: 1);
        var defender = BattleTestData.CreateUnit();
        var battle = BattleAggregate.Create([attacker, defender]).Value;

        battle.Attack(attacker.Id, defender.Id);
        var firstEvent = battle.GetDomainEvents().ShouldHaveSingleItem().ShouldBeOfType<UnitDamaged>();
        battle.Attack(attacker.Id, defender.Id);

        firstEvent.CurrentUnitHealth.ShouldBe(9);
        defender.CurrentUnitHealth.ShouldBe(8);
        battle.GetDomainEvents().Count.ShouldBe(2);
    }
}
