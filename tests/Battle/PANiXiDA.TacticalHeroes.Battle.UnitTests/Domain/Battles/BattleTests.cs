using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Heroes;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Heroes.ValueObjects;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Enumerations;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Events;
using PANiXiDA.TacticalHeroes.Battle.Domain.Factions;
using PANiXiDA.TacticalHeroes.Battle.Domain.Games;
using PANiXiDA.TacticalHeroes.Battle.Domain.Players;
using PANiXiDA.TacticalHeroes.Battle.UnitTests.Battles;

using BattleAggregate = PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Battle;
using HeroDefinitionId = PANiXiDA.TacticalHeroes.Battle.Domain.Heroes.HeroId;

namespace PANiXiDA.TacticalHeroes.Battle.UnitTests.Domain.Battles;

public sealed class BattleTests
{
    [Fact(DisplayName = "Battle should prepare two players when created from game")]
    public void Create_Should_PrepareTwoPlayers_When_CreatedFromGame()
    {
        var game = GameId.New();
        var first = PlayerId.New();
        var second = PlayerId.New();

        var battle = BattleAggregate.Create(game, first, second).Value;

        battle.GameId.ShouldBe(game);
        battle.FirstPlayerId.ShouldBe(first);
        battle.SecondPlayerId.ShouldBe(second);
        battle.Status.ShouldBe(BattleStatus.Preparing);
        battle.GetDomainEvents().ShouldHaveSingleItem().ShouldBeOfType<BattleCreated>();
    }

    [Fact(DisplayName = "Battle should reject identical players when players are equal")]
    public void Create_Should_RejectPlayers_When_PlayersAreEqual()
    {
        var player = PlayerId.New();

        var result = BattleAggregate.Create(GameId.New(), player, player);

        result.IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Battle should allow combat only when preparation is complete")]
    public void Start_Should_AllowCombat_When_PreparationIsComplete()
    {
        var battle = BattleAggregate.Create(GameId.New(), PlayerId.New(), PlayerId.New()).Value;

        var result = battle.Start();

        result.IsSuccess.ShouldBeTrue();
        battle.Status.ShouldBe(BattleStatus.InProgress);
        battle.Start().IsSuccess.ShouldBeTrue();
    }

    [Fact(DisplayName = "Battle should preserve ownership when hero is added")]
    public void AddHeroes_Should_PreserveOwnership_When_HeroIsAdded()
    {
        var owner = PlayerId.New();
        var battle = BattleAggregate.Create(GameId.New(), owner, PlayerId.New()).Value;
        var hero = Hero.Create(
            battleId: battle.Id,
            ownerPlayerId: owner,
            definitionId: HeroDefinitionId.New(),
            factionId: FactionId.New(),
            stats: HeroCombatStats.Create(
                attack: 1,
                defense: 2,
                minimumDamage: 1,
                maximumDamage: 2,
                initiative: 10,
                morale: 1,
                luck: 1).Value);

        var result = battle.AddHeroes([hero]);

        result.IsSuccess.ShouldBeTrue();
        battle.Heroes.ShouldHaveSingleItem().OwnerPlayerId.ShouldBe(owner);
    }

    [Fact(DisplayName = "Battle should reject units without an owner when created for game")]
    public void AddUnits_Should_RejectUnownedUnits_When_CreatedForGame()
    {
        var battle = BattleAggregate.Create(GameId.New(), PlayerId.New(), PlayerId.New()).Value;
        var unit = BattleTestData.CreateUnit(battle.Id);

        var result = battle.AddUnits([unit]);

        result.IsFailure.ShouldBeTrue();
        battle.Units.ShouldBeEmpty();
    }

    [Fact(DisplayName = "Battle should generate its identity when a battle is created")]
    public void Create_Should_GenerateIdentity_When_ABattleIsCreated()
    {
        var battle = BattleAggregate.Create();

        battle.Id.Value.ShouldNotBe(Guid.Empty);
        battle.Units.ShouldBeEmpty();
    }

    [Fact(DisplayName = "Battle should add its units when participants belong to the battle")]
    public void AddUnits_Should_AddUnits_When_ParticipantsBelongToTheBattle()
    {
        var battle = BattleAggregate.Create();
        var first = BattleTestData.CreateUnit(battle.Id);
        var second = BattleTestData.CreateUnit(battle.Id);
        var input = new List<Unit> { first, second };

        var result = battle.AddUnits(input);
        input.Clear();

        result.IsSuccess.ShouldBeTrue();
        battle.Units.Count.ShouldBe(2);
        battle.Units.ShouldContain(first);
        battle.Units.ShouldContain(second);
        battle.Units.ShouldAllBe(unit => unit.BattleId == battle.Id);
        Should.Throw<NotSupportedException>(() => ((ICollection<Unit>)battle.Units).Clear());
    }

    [Fact(DisplayName = "Battle should reject participants when participants are duplicated")]
    public void AddUnits_Should_ReturnFailure_When_ParticipantsAreDuplicated()
    {
        var battle = BattleAggregate.Create();
        var unit = BattleTestData.CreateUnit(battle.Id);

        var result = battle.AddUnits([unit, unit]);

        result.IsFailure.ShouldBeTrue();
        battle.Units.ShouldBeEmpty();
        unit.BattleId.ShouldBe(battle.Id);
    }

    [Fact(DisplayName = "Battle should preserve its units when a participant is null")]
    public void AddUnits_Should_PreserveUnits_When_AParticipantIsNull()
    {
        var battle = BattleAggregate.Create();
        var unit = BattleTestData.CreateUnit(battle.Id);

        var result = battle.AddUnits([unit, null!]);

        result.IsFailure.ShouldBeTrue();
        battle.Units.ShouldBeEmpty();
    }

    [Theory(DisplayName = "Battle should accept a batch when batch contains fewer than two units")]
    [InlineData(0)]
    [InlineData(1)]
    public void AddUnits_Should_AcceptBatch_When_BatchContainsFewerThanTwoUnits(int count)
    {
        var battle = BattleAggregate.Create();
        var units = Enumerable.Range(0, count).Select(_ => BattleTestData.CreateUnit(battle.Id));

        var result = battle.AddUnits(units);

        result.IsSuccess.ShouldBeTrue();
        battle.Units.Count.ShouldBe(count);
    }

    [Fact(DisplayName = "Battle should reject the entire batch when a participant belongs to another battle")]
    public void AddUnits_Should_RejectEntireBatch_When_AParticipantBelongsToAnotherBattle()
    {
        var battle = BattleAggregate.Create();
        var otherBattle = BattleAggregate.Create();
        var own = BattleTestData.CreateUnit(battle.Id);
        var foreign = BattleTestData.CreateUnit(otherBattle.Id);
        otherBattle.AddUnits([foreign]).IsSuccess.ShouldBeTrue();

        var result = battle.AddUnits([own, foreign]);

        result.IsFailure.ShouldBeTrue();
        battle.Units.ShouldBeEmpty();
        otherBattle.Units.ShouldHaveSingleItem().ShouldBe(foreign);
        foreign.BattleId.ShouldBe(otherBattle.Id);
    }

    [Fact(DisplayName = "Battle should reject the entire batch when a participant is already present")]
    public void AddUnits_Should_RejectEntireBatch_When_AParticipantIsAlreadyPresent()
    {
        var battle = BattleAggregate.Create();
        var existing = BattleTestData.CreateUnit(battle.Id);
        var added = BattleTestData.CreateUnit(battle.Id);
        battle.AddUnits([existing]).IsSuccess.ShouldBeTrue();

        var result = battle.AddUnits([added, existing]);

        result.IsFailure.ShouldBeTrue();
        battle.Units.ShouldHaveSingleItem().ShouldBe(existing);
    }

    [Fact(DisplayName = "Battle should append units when a subsequent batch is valid")]
    public void AddUnits_Should_AppendUnits_When_ASubsequentBatchIsValid()
    {
        var battle = BattleAggregate.Create();
        var first = BattleTestData.CreateUnit(battle.Id);
        var second = BattleTestData.CreateUnit(battle.Id);
        battle.AddUnits([first]).IsSuccess.ShouldBeTrue();

        var result = battle.AddUnits([second]);

        result.IsSuccess.ShouldBeTrue();
        battle.Units.Count.ShouldBe(2);
        battle.Units.ShouldContain(first);
        battle.Units.ShouldContain(second);
    }

    [Fact(DisplayName = "Attack should damage only the defender when attack is valid")]
    public void Attack_Should_DamageOnlyDefender_When_AttackIsValid()
    {
        var battle = BattleAggregate.Create();
        var attacker = BattleTestData.CreateUnit(battle.Id, minimumDamage: 7, maximumDamage: 7, count: 1);
        var defender = BattleTestData.CreateUnit(battle.Id, currentHealth: 4);
        battle.AddUnits([attacker, defender]).IsSuccess.ShouldBeTrue();

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
        var battle = BattleAggregate.Create();
        var attacker = BattleTestData.CreateUnit(battle.Id, minimumDamage: 7, maximumDamage: 7, count: 1);
        var defender = BattleTestData.CreateUnit(battle.Id, count: 1, currentHealth: 4);
        battle.AddUnits([attacker, defender]).IsSuccess.ShouldBeTrue();

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
        var battle = BattleAggregate.Create();
        var unit = BattleTestData.CreateUnit(battle.Id);
        battle.AddUnits([unit, BattleTestData.CreateUnit(battle.Id)]).IsSuccess.ShouldBeTrue();

        var result = battle.Attack(unit.Id, unit.Id);

        result.IsFailure.ShouldBeTrue();
        unit.TotalHealth.ShouldBe(30);
        battle.GetDomainEvents().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Attack should preserve units when participant belongs to another battle")]
    public void Attack_Should_PreserveUnits_When_ParticipantBelongsToAnotherBattle()
    {
        var battle = BattleAggregate.Create();
        var attacker = BattleTestData.CreateUnit(battle.Id);
        var defender = BattleTestData.CreateUnit(battle.Id);
        battle.AddUnits([attacker, defender]).IsSuccess.ShouldBeTrue();
        var otherBattle = BattleAggregate.Create();
        var foreign = BattleTestData.CreateUnit(otherBattle.Id);
        otherBattle.AddUnits([foreign]).IsSuccess.ShouldBeTrue();

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
        var battle = BattleAggregate.Create();
        var attacker = BattleTestData.CreateUnit(battle.Id, count: destroyedAttacker ? 0 : 1);
        var defender = BattleTestData.CreateUnit(battle.Id, count: destroyedAttacker ? 1 : 0);
        battle.AddUnits([attacker, defender]).IsSuccess.ShouldBeTrue();

        var result = battle.Attack(attacker.Id, defender.Id);

        result.IsFailure.ShouldBeTrue();
        battle.GetDomainEvents().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Attack should collect both errors when both participants are missing")]
    public void Attack_Should_CollectBothErrors_When_BothParticipantsAreMissing()
    {
        var battle = BattleAggregate.Create();
        var attackerId = UnitId.New();
        var defenderId = UnitId.New();

        var result = battle.Attack(attackerId, defenderId);

        result.IsFailure.ShouldBeTrue();
        result.Errors.Count.ShouldBe(2);
        result.Errors.ShouldContain(error => error.Message.Contains(attackerId.ToString(), StringComparison.Ordinal));
        result.Errors.ShouldContain(error => error.Message.Contains(defenderId.ToString(), StringComparison.Ordinal));
        result.Errors.ShouldAllBe(error => error.Type == ErrorType.Validation);
        battle.GetDomainEvents().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Attack should collect both errors when both participants are destroyed")]
    public void Attack_Should_CollectBothErrors_When_BothParticipantsAreDestroyed()
    {
        var battle = BattleAggregate.Create();
        var attacker = BattleTestData.CreateUnit(battle.Id, count: 0);
        var defender = BattleTestData.CreateUnit(battle.Id, count: 0);
        battle.AddUnits([attacker, defender]).IsSuccess.ShouldBeTrue();

        var result = battle.Attack(attacker.Id, defender.Id);

        result.IsFailure.ShouldBeTrue();
        result.Errors.Count.ShouldBe(2);
        result.Errors.ShouldContain(error => error.Message.Contains(
            attacker.Id.ToString(),
            StringComparison.Ordinal));
        result.Errors.ShouldContain(error => error.Message.Contains(
            defender.Id.ToString(),
            StringComparison.Ordinal));
        attacker.TotalHealth.ShouldBe(0);
        defender.TotalHealth.ShouldBe(0);
        battle.GetDomainEvents().ShouldBeEmpty();
    }

    [Theory(DisplayName = "Attack should preserve the existing unit when one participant is missing")]
    [InlineData(true)]
    [InlineData(false)]
    public void Attack_Should_PreserveExistingUnit_When_OneParticipantIsMissing(bool missingAttacker)
    {
        var battle = BattleAggregate.Create();
        var unit = BattleTestData.CreateUnit(battle.Id);
        battle.AddUnits([unit]).IsSuccess.ShouldBeTrue();
        var missingId = UnitId.New();
        var attackerId = missingAttacker ? missingId : unit.Id;
        var defenderId = missingAttacker ? unit.Id : missingId;

        var result = battle.Attack(attackerId, defenderId);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldHaveSingleItem().Message.ShouldContain(missingId.ToString());
        unit.TotalHealth.ShouldBe(30);
        battle.GetDomainEvents().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Attack should select damage within bounds when random damage is configured")]
    public void Attack_Should_SelectDamageWithinBounds_When_RandomDamageIsConfigured()
    {
        var battle = BattleAggregate.Create();
        var attacker = BattleTestData.CreateUnit(battle.Id, minimumDamage: 2, maximumDamage: 4, count: 1);
        var defender = BattleTestData.CreateUnit(battle.Id);
        battle.AddUnits([attacker, defender]).IsSuccess.ShouldBeTrue();

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
        var battle = BattleAggregate.Create();
        var attacker = BattleTestData.CreateUnit(
            battle.Id,
            attack: int.MaxValue,
            minimumDamage: int.MaxValue,
            maximumDamage: int.MaxValue,
            count: int.MaxValue);
        var defender = BattleTestData.CreateUnit(battle.Id);
        battle.AddUnits([attacker, defender]).IsSuccess.ShouldBeTrue();

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
        var battle = BattleAggregate.Create();
        var attacker = BattleTestData.CreateUnit(battle.Id, minimumDamage: 0, maximumDamage: 0);
        var defender = BattleTestData.CreateUnit(battle.Id);
        battle.AddUnits([attacker, defender]).IsSuccess.ShouldBeTrue();

        var result = battle.Attack(attacker.Id, defender.Id);

        result.IsSuccess.ShouldBeTrue();
        defender.TotalHealth.ShouldBe(30);
        battle.GetDomainEvents().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Attack should preserve past events when another hit changes the target")]
    public void Attack_Should_PreservePastEvents_When_AnotherHitChangesTheTarget()
    {
        var battle = BattleAggregate.Create();
        var attacker = BattleTestData.CreateUnit(battle.Id, count: 1);
        var defender = BattleTestData.CreateUnit(battle.Id);
        battle.AddUnits([attacker, defender]).IsSuccess.ShouldBeTrue();

        battle.Attack(attacker.Id, defender.Id);
        var firstEvent = battle.GetDomainEvents().ShouldHaveSingleItem().ShouldBeOfType<UnitDamaged>();
        battle.Attack(attacker.Id, defender.Id);

        firstEvent.CurrentUnitHealth.ShouldBe(9);
        defender.CurrentUnitHealth.ShouldBe(8);
        battle.GetDomainEvents().Count.ShouldBe(2);
    }
}
