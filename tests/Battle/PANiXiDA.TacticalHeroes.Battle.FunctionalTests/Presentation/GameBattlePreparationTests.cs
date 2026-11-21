using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Persistence;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Abstractions;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Enumerations;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Events;
using PANiXiDA.TacticalHeroes.Builds.Domain.Builds;
using PANiXiDA.TacticalHeroes.Builds.Domain.Builds.Abstractions;
using PANiXiDA.TacticalHeroes.Builds.Domain.Builds.ValueObjects;
using PANiXiDA.TacticalHeroes.Builds.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.ValueObjects;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes.ValueObjects;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Units;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Units.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Units.ValueObjects;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Games.Domain.Games;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Abstractions;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Entities.GameParticipants.ValueObjects;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Enumerations;
using PANiXiDA.TacticalHeroes.Games.Infrastructure.Persistence.Core;

using Wolverine.Tracking;

using BattleGameId = PANiXiDA.TacticalHeroes.Battle.Domain.Games.GameId;
using BuildHeroId = PANiXiDA.TacticalHeroes.Builds.Domain.Heroes.HeroId;
using BuildPlayerId = PANiXiDA.TacticalHeroes.Builds.Domain.Players.PlayerId;
using BuildUnitId = PANiXiDA.TacticalHeroes.Builds.Domain.Units.UnitId;
using GameBuildId = PANiXiDA.TacticalHeroes.Games.Domain.Builds.BuildId;
using GamePlayerId = PANiXiDA.TacticalHeroes.Games.Domain.Players.PlayerId;
using MatchSearchId = PANiXiDA.TacticalHeroes.Games.Domain.MatchSearches.MatchSearchId;

namespace PANiXiDA.TacticalHeroes.Battle.FunctionalTests.Presentation;

[Collection(FunctionalTestCollectionDefinition.Name)]
public sealed class GameBattlePreparationTests(FunctionalTestFixture fixture)
{
    [Fact(DisplayName = "Battle should prepare armies and assign the waiting game when creation is requested")]
    public async Task PublishAsync_Should_PrepareArmiesAndAssignGame_When_CreationIsRequested()
    {
        var ct = TestContext.Current.CancellationToken;
        var data = await SeedAsync(ct);
        var request = CreateRequest(data.Game);

        await fixture.Services.TrackActivity().Timeout(TimeSpan.FromSeconds(30)).PublishMessageAndWaitAsync(request);

        await using var scope = fixture.Services.CreateAsyncScope();
        var battle = await scope.ServiceProvider.GetRequiredService<IBattlesRepository>()
            .GetByGameIdAsync(BattleGameId.Create(data.Game.Id.Value).Value, ct);
        battle.ShouldNotBeNull();
        battle.Status.ShouldBe(BattleStatus.Preparing);
        battle.Units.Count.ShouldBe(2);
        battle.Heroes.Count.ShouldBe(2);
        battle.Units.ShouldAllBe(unit => unit.DefinitionId!.Value.Value == data.Unit.Id.Value
            && unit.Stats.Attack == data.Unit.Stats.Attack
            && unit.Traits!.Shots == data.Unit.RangedAttack.Shots
            && unit.Count == 1
            && unit.CurrentUnitHealth == data.Unit.Stats.Health);
        battle.Heroes.ShouldAllBe(hero => hero.DefinitionId.Value == data.Hero.Id.Value
            && hero.Stats.Initiative == data.Hero.Stats.Initiative);
        battle.Units.Select(unit => unit.OwnerPlayerId!.Value.Value).Order().ShouldBe(
            data.Game.Participants.Select(player => player.PlayerId.Value).Order());
        var game = await scope.ServiceProvider.GetRequiredService<IGamesRepository>().GetByIdAsync(data.Game.Id, ct);
        game.ShouldNotBeNull();
        game.BattleId!.Value.Value.ShouldBe(battle.Id.Value);
        game.Status.ShouldBe(GameStatus.WaitingForPlayers);
        game.StartedAt.ShouldBeNull();
    }

    [Fact(DisplayName = "Creation should preserve snapshots when selections change and request repeats")]
    public async Task PublishAsync_Should_PreserveSnapshot_When_SelectionsChangeAndRequestRepeats()
    {
        var ct = TestContext.Current.CancellationToken;
        var data = await SeedAsync(ct);
        var request = CreateRequest(data.Game);
        await fixture.Services.TrackActivity().Timeout(TimeSpan.FromSeconds(30)).PublishMessageAndWaitAsync(request);
        var originalAttack = data.Unit.Stats.Attack;
        await using (var editScope = fixture.Services.CreateAsyncScope())
        {
            var catalog = editScope.ServiceProvider.GetRequiredService<CompendiumWriteDbContext>();
            await catalog.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE compendium.units SET stats_attack = {99} WHERE id = {data.Unit.Id.Value}",
                ct);
            var builds = editScope.ServiceProvider.GetRequiredService<BuildsWriteDbContext>();
            await builds.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM builds.build_units WHERE build_id = {data.FirstBuild.Id.Value}",
                ct);
        }

        await fixture.Services.TrackActivity().Timeout(TimeSpan.FromSeconds(30))
            .PublishMessageAndWaitAsync(CreateRequest(data.Game));

        await using var scope = fixture.Services.CreateAsyncScope();
        var battle = await scope.ServiceProvider.GetRequiredService<IBattlesRepository>()
            .GetByGameIdAsync(BattleGameId.Create(data.Game.Id.Value).Value, ct);
        battle.ShouldNotBeNull();
        battle.Units.Count.ShouldBe(2);
        battle.Units.ShouldAllBe(unit => unit.Stats.Attack == originalAttack);
        battle.Heroes.Count.ShouldBe(2);
        battle.Status.ShouldBe(BattleStatus.Preparing);
    }

    private static BattleCreationRequested CreateRequest(Game game)
    {
        var players = game.Participants.ToArray();
        return new BattleCreationRequested(
            GameId: game.Id.Value,
            FirstPlayerId: players[0].PlayerId.Value,
            FirstBuildId: players[0].BuildId.Value,
            SecondPlayerId: players[1].PlayerId.Value,
            SecondBuildId: players[1].BuildId.Value);
    }

    private async Task<SeededGame> SeedAsync(CancellationToken cancellationToken)
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var faction = Faction.Create(
            name: FactionName.Create("Preparation faction").Value,
            description: FactionDescription.Create("Faction for battle preparation.").Value);
        var unit = Unit.Create(
            name: UnitName.Create("Preparation archer").Value,
            description: UnitDescription.Create("A ranged unit.").Value,
            stats: UnitCombatStats.Create(
                attack: 8,
                defense: 4,
                health: 12,
                minimumDamage: 3,
                maximumDamage: 5,
                initiative: 10.5,
                speed: 6).Value,
            rangedAttack: UnitRangedAttack.Create(shots: 12, rangedAttackRange: 8).Value,
            morale: UnitMorale.Create(2).Value,
            luck: UnitLuck.Create(1).Value,
            factionId: faction.Id);
        var hero = Hero.Create(
            name: HeroName.Create("Preparation hero").Value,
            description: HeroDescription.Create("An army commander.").Value,
            stats: HeroCombatStats.Create(
                attack: 4,
                defense: 5,
                minimumDamage: 2,
                maximumDamage: 4,
                initiative: 9).Value,
            morale: HeroMorale.Create(2).Value,
            luck: HeroLuck.Create(1).Value,
            factionId: faction.Id);
        scope.ServiceProvider.GetRequiredService<IFactionsRepository>().Add(faction);
        scope.ServiceProvider.GetRequiredService<IUnitsRepository>().Add(unit);
        scope.ServiceProvider.GetRequiredService<IHeroesRepository>().Add(hero);
        await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(CompendiumWriteDbContext))
            .SaveChangesAsync(cancellationToken);
        var firstPlayer = GamePlayerId.New();
        var secondPlayer = GamePlayerId.New();
        var firstBuild = Build.Create(
            BuildPlayerId.Create(firstPlayer.Value).Value,
            BuildName.Create("First army").Value);
        var secondBuild = Build.Create(
            BuildPlayerId.Create(secondPlayer.Value).Value,
            BuildName.Create("Second army").Value);
        foreach (var build in new[] { firstBuild, secondBuild })
        {
            build.AddUnit(BuildUnitId.Create(unit.Id.Value).Value).IsSuccess.ShouldBeTrue();
            build.AddHero(BuildHeroId.Create(hero.Id.Value).Value).IsSuccess.ShouldBeTrue();
            scope.ServiceProvider.GetRequiredService<IBuildsRepository>().Add(build);
        }

        await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(BuildsWriteDbContext))
            .SaveChangesAsync(cancellationToken);
        var game = Game.Create(
            mode: GameMode.RankedDuel,
            firstParticipant: GameParticipantSelection.Create(
                firstPlayer,
                GameBuildId.Create(firstBuild.Id.Value).Value,
                Rating.Create(1500)),
            secondParticipant: GameParticipantSelection.Create(
                secondPlayer,
                GameBuildId.Create(secondBuild.Id.Value).Value,
                Rating.Create(1600)),
            firstMatchSearchId: MatchSearchId.New(),
            secondMatchSearchId: MatchSearchId.New()).Value;
        scope.ServiceProvider.GetRequiredService<IGamesRepository>().Add(game);
        await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(GamesWriteDbContext))
            .SaveChangesAsync(cancellationToken);
        return new SeededGame(game, unit, hero, firstBuild);
    }

    private sealed record SeededGame(Game Game, Unit Unit, Hero Hero, Build FirstBuild);
}
