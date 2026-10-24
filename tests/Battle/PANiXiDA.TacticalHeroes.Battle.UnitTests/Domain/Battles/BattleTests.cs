using PANiXiDA.TacticalHeroes.Battle.Domain.Battles;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units.ValueObjects;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Events;
using PANiXiDA.TacticalHeroes.Battle.UnitTests.Battles;

using BattleAggregate = PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Battle;

namespace PANiXiDA.TacticalHeroes.Battle.UnitTests.Domain.Battles;

public sealed class BattleTests
{
    [Fact(DisplayName = "Battle should preserve its identity when an existing identifier is supplied")]
    public void Create_Should_PreserveIdentity_When_AnExistingIdentifierIsSupplied()
    {
        var id = BattleId.New();

        var battle = BattleAggregate.Create(
            id, [BattleTestData.CreateUnit(), BattleTestData.CreateUnit()]).Value;

        battle.Id.ShouldBe(id);
        battle.Units.ShouldAllBe(unit => unit.BattleId == id);
    }

    [Fact(DisplayName = "Battle should reject its identity when identifier is empty")]
    public void Create_Should_ReturnFailure_When_IdentifierIsEmpty()
    {
        var first = BattleTestData.CreateUnit();
        var second = BattleTestData.CreateUnit();

        var result = BattleAggregate.Create(default, [first, second]);

        result.IsFailure.ShouldBeTrue();
        first.BattleId.ShouldBeNull();
        second.BattleId.ShouldBeNull();
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

    [Fact(DisplayName = "Battle should reject identities when different participants share an identifier")]
    public void Create_Should_RejectIdentities_When_DifferentParticipantsShareAnIdentifier()
    {
        var first = BattleTestData.CreateUnit();
        var second = Unit.Create(first.Id, first.Stats, first.DamageRange, first.StackState).Value;

        var result = BattleAggregate.Create([first, second]);

        result.IsFailure.ShouldBeTrue();
        first.BattleId.ShouldBeNull();
        second.BattleId.ShouldBeNull();
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

        var result = battle.Attack(attacker.Id, defender.Id, DamageRoll.Create(7).Value).Value;

        attacker.TotalHealth.ShouldBe(10);
        defender.Count.ShouldBe(2);
        defender.CurrentUnitHealth.ShouldBe(7);
        result.AppliedDamage.Value.ShouldBe(7);
        var domainEvent = battle.GetDomainEvents().ShouldHaveSingleItem().ShouldBeOfType<UnitDamaged>();
        domainEvent.BattleId.ShouldBe(battle.Id);
        domainEvent.AttackerId.ShouldBe(attacker.Id);
        domainEvent.DefenderId.ShouldBe(defender.Id);
        domainEvent.Result.ShouldBe(result);
    }

    [Fact(DisplayName = "Attack should reject the attacker when attacker targets itself")]
    public void Attack_Should_ReturnFailure_When_AttackerTargetsItself()
    {
        var unit = BattleTestData.CreateUnit();
        var battle = BattleAggregate.Create([unit, BattleTestData.CreateUnit()]).Value;

        var result = battle.Attack(unit.Id, unit.Id, DamageRoll.Create(1).Value);

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

        var result = battle.Attack(attacker.Id, foreign.Id, DamageRoll.Create(1).Value);

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

        var result = battle.Attack(attacker.Id, defender.Id, DamageRoll.Create(1).Value);

        result.IsFailure.ShouldBeTrue();
        battle.GetDomainEvents().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Attack should preserve state when roll is outside damage range")]
    public void Attack_Should_PreserveState_When_RollIsOutsideDamageRange()
    {
        var attacker = BattleTestData.CreateUnit();
        var defender = BattleTestData.CreateUnit();
        var battle = BattleAggregate.Create([attacker, defender]).Value;

        var result = battle.Attack(attacker.Id, defender.Id, DamageRoll.Create(2).Value);

        result.IsFailure.ShouldBeTrue();
        defender.TotalHealth.ShouldBe(30);
        battle.GetDomainEvents().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Attack should avoid damage events when calculated damage is zero")]
    public void Attack_Should_NotRaiseDamageEvent_When_CalculatedDamageIsZero()
    {
        var attacker = BattleTestData.CreateUnit(minimumDamage: 0, maximumDamage: 0);
        var defender = BattleTestData.CreateUnit();
        var battle = BattleAggregate.Create([attacker, defender]).Value;

        var result = battle.Attack(attacker.Id, defender.Id, DamageRoll.Create(0).Value);

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

        battle.Attack(attacker.Id, defender.Id, DamageRoll.Create(1).Value);
        var firstEvent = battle.GetDomainEvents().ShouldHaveSingleItem().ShouldBeOfType<UnitDamaged>();
        battle.Attack(attacker.Id, defender.Id, DamageRoll.Create(1).Value);

        firstEvent.Result.RemainingStack.CurrentUnitHealth.ShouldBe(9);
        defender.CurrentUnitHealth.ShouldBe(8);
        battle.GetDomainEvents().Count.ShouldBe(2);
    }
}
