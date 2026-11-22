using FluentValidation;

using PANiXiDA.TacticalHeroes.Battle.Application.Abstractions;
using PANiXiDA.TacticalHeroes.Battle.Application.Battles.Events;
using PANiXiDA.TacticalHeroes.Battle.Application.ReadModels;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Abstractions;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Enumerations;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Events;
using PANiXiDA.TacticalHeroes.Battle.Domain.Games;
using PANiXiDA.TacticalHeroes.Battle.Domain.Players;
using PANiXiDA.TacticalHeroes.Battle.UnitTests.Battles;

using BattleAggregate = PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Battle;

namespace PANiXiDA.TacticalHeroes.Battle.UnitTests.Application.Battles.Events;

public sealed class BattleCreationRequestedHandlerTests
{
    [Fact(DisplayName = "Handler should capture both armies and record creation when game is created")]
    public async Task HandleAsync_Should_CaptureBothArmies_When_GameIsCreated()
    {
        var ct = TestContext.Current.CancellationToken;
        var request = BattlePreparationTestData.CreateRequest();
        var unit = BattlePreparationTestData.CreateUnit();
        var hero = BattlePreparationTestData.CreateHero();
        var repository = Substitute.For<IBattlesRepository>();
        var builds = Substitute.For<IBuildsClient>();
        var compendium = Substitute.For<ICompendiumClient>();
        builds.GetByIdAsync(request.FirstBuildId, ct).Returns(
            new BuildReadModel(request.FirstBuildId, request.FirstPlayerId, [hero.Id], [unit.Id]));
        builds.GetByIdAsync(request.SecondBuildId, ct).Returns(
            new BuildReadModel(request.SecondBuildId, request.SecondPlayerId, [hero.Id], [unit.Id]));
        compendium.GetUnitByIdAsync(unit.Id, ct).Returns(unit);
        compendium.GetHeroByIdAsync(hero.Id, ct).Returns(hero);
        BattleAggregate? captured = null;
        repository.When(item => item.Add(Arg.Any<BattleAggregate>()))
            .Do(call => captured = call.Arg<BattleAggregate>());
        var handler = new BattleCreationRequestedHandler(repository, builds, compendium);

        await handler.HandleAsync(request, ct);

        captured.ShouldNotBeNull();
        captured.GameId!.Value.Value.ShouldBe(request.GameId);
        captured.Status.ShouldBe(BattleStatus.Preparing);
        captured.Units.Count.ShouldBe(2);
        captured.Heroes.Count.ShouldBe(2);
        captured.Units.Select(item => item.OwnerPlayerId!.Value.Value).Order()
            .ShouldBe(new[] { request.FirstPlayerId, request.SecondPlayerId }.Order());
        captured.Units.ShouldAllBe(item => item.DefinitionId!.Value.Value == unit.Id
            && item.Stats.Attack == unit.Attack
            && item.Count == 1
            && item.CurrentUnitHealth == unit.Health
            && item.DamageRange.Minimum == unit.MinimumDamage
            && item.DamageRange.Maximum == unit.MaximumDamage
            && item.Traits!.Initiative == unit.Initiative
            && item.Traits.Shots == unit.Shots);
        captured.Heroes.ShouldAllBe(item => item.DefinitionId.Value == hero.Id && item.Stats.Attack == hero.Attack);
        var created = captured.GetDomainEvents().ShouldHaveSingleItem().ShouldBeOfType<BattleCreated>();
        created.GameId.ShouldBe(request.GameId);
        captured.Attack(captured.Units.First().Id, captured.Units.Last().Id).IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Handler should preserve an existing snapshot when creation is repeated")]
    public async Task HandleAsync_Should_PreserveExistingSnapshot_When_CreationIsRepeated()
    {
        var ct = TestContext.Current.CancellationToken;
        var request = BattlePreparationTestData.CreateRequest();
        var repository = Substitute.For<IBattlesRepository>();
        var builds = Substitute.For<IBuildsClient>();
        var compendium = Substitute.For<ICompendiumClient>();
        var existing = BattleAggregate.Create(
            GameId.Create(request.GameId).Value,
            PlayerId.Create(request.FirstPlayerId).Value,
            PlayerId.Create(request.SecondPlayerId).Value).Value;
        repository.GetByGameIdAsync(GameId.Create(request.GameId).Value, ct).Returns(existing);
        var handler = new BattleCreationRequestedHandler(repository, builds, compendium);

        await handler.HandleAsync(request, ct);

        repository.DidNotReceive().Add(Arg.Any<BattleAggregate>());
        builds.ReceivedCalls().ShouldBeEmpty();
        compendium.ReceivedCalls().ShouldBeEmpty();
    }

    [Theory(DisplayName = "Handler should reject unavailable builds when selection is invalid")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_RejectBuild_When_SelectionIsInvalid(bool exists)
    {
        var ct = TestContext.Current.CancellationToken;
        var request = BattlePreparationTestData.CreateRequest();
        var repository = Substitute.For<IBattlesRepository>();
        var builds = Substitute.For<IBuildsClient>();
        builds.GetByIdAsync(request.FirstBuildId, ct).Returns(exists
            ? new BuildReadModel(request.FirstBuildId, Guid.CreateVersion7(), [], []) : null);
        var handler = new BattleCreationRequestedHandler(repository, builds, Substitute.For<ICompendiumClient>());

        await Should.ThrowAsync<InvalidOperationException>(() => handler.HandleAsync(request, ct));

        repository.DidNotReceive().Add(Arg.Any<BattleAggregate>());
    }

    [Fact(DisplayName = "Handler should reject malformed identifiers before persistence when request is invalid")]
    public async Task HandleAsync_Should_RejectIdentifiers_When_RequestIsInvalid()
    {
        var repository = Substitute.For<IBattlesRepository>();
        var request = BattlePreparationTestData.CreateRequest() with { GameId = Guid.Empty };
        var handler = new BattleCreationRequestedHandler(
            repository,
            Substitute.For<IBuildsClient>(),
            Substitute.For<ICompendiumClient>());

        await Should.ThrowAsync<ValidationException>(() => handler.HandleAsync(
            request,
            TestContext.Current.CancellationToken));

        repository.ReceivedCalls().ShouldBeEmpty();
    }
}
