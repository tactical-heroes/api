using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Update;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.UnitTests.Heroes;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Application.Heroes.Update;

public sealed class UpdateHeroHandlerTests
{
    [Fact(DisplayName = "Update hero handler should update an existing hero when hero and faction exist")]
    public async Task HandleAsync_Should_UpdateHero_When_HeroAndFactionExist()
    {
        var faction = HeroTestData.CreateFaction();
        var hero = HeroTestData.CreateHero(faction);
        var heroesRepository = Substitute.For<IHeroesRepository>();
        heroesRepository.GetByIdAsync(hero.Id, Arg.Any<CancellationToken>())
            .Returns(hero);
        var factionsRepository = Substitute.For<IFactionsRepository>();
        factionsRepository.GetByIdAsync(
                faction.Id,
                Arg.Any<CancellationToken>())
            .Returns(faction);
        var handler = new UpdateHeroHandler(
            heroesRepository,
            factionsRepository);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            HeroTestData.CreateUpdateCommand(
                hero.Id.Value,
                faction.Id.Value),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        hero.Name.Value.ShouldBe("Elara");
        hero.Stats.Attack.ShouldBe(10);
        hero.Stats.MaximumDamage.ShouldBe(9);
        hero.Morale.Value.ShouldBe(5);
        await heroesRepository.Received(1).UpdateAsync(hero, cancellationToken);
    }

    [Fact(DisplayName = "Update hero handler should return not found when hero does not exist")]
    public async Task HandleAsync_Should_ReturnNotFound_When_HeroDoesNotExist()
    {
        var heroesRepository = Substitute.For<IHeroesRepository>();
        heroesRepository.GetByIdAsync(
                Arg.Any<HeroId>(),
                Arg.Any<CancellationToken>())
            .Returns((Hero?)null);
        var handler = new UpdateHeroHandler(
            heroesRepository,
            Substitute.For<IFactionsRepository>());

        var result = await handler.HandleAsync(
            HeroTestData.CreateUpdateCommand(
                Guid.CreateVersion7(),
                Guid.CreateVersion7()),
            TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(
            ErrorType.NotFound,
            "Hero was not found.");
    }

    [Fact(DisplayName = "Update hero handler should return not found when faction does not exist")]
    public async Task HandleAsync_Should_ReturnNotFound_When_FactionDoesNotExist()
    {
        var faction = HeroTestData.CreateFaction();
        var hero = HeroTestData.CreateHero(faction);
        var heroesRepository = Substitute.For<IHeroesRepository>();
        heroesRepository.GetByIdAsync(hero.Id, Arg.Any<CancellationToken>())
            .Returns(hero);
        var factionsRepository = Substitute.For<IFactionsRepository>();
        factionsRepository.GetByIdAsync(
                Arg.Any<FactionId>(),
                Arg.Any<CancellationToken>())
            .Returns((Faction?)null);
        var handler = new UpdateHeroHandler(
            heroesRepository,
            factionsRepository);

        var result = await handler.HandleAsync(
            HeroTestData.CreateUpdateCommand(
                hero.Id.Value,
                Guid.CreateVersion7()),
            TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(
            ErrorType.NotFound,
            "Faction was not found.");
        hero.Name.Value.ShouldBe("Orrin");
        await heroesRepository.DidNotReceiveWithAnyArgs()
            .UpdateAsync(null!, TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Update hero handler should reject invalid values without saving when command is invalid")]
    public async Task HandleAsync_Should_ReturnValidationFailuresWithoutSaving_When_CommandIsInvalid()
    {
        var faction = HeroTestData.CreateFaction();
        var repository = Substitute.For<IHeroesRepository>();
        var factionsRepository = Substitute.For<IFactionsRepository>();
        var hero = HeroTestData.CreateHero(faction);
        var originalName = hero.Name;
        var originalStats = hero.Stats;
        repository.GetByIdAsync(hero.Id, Arg.Any<CancellationToken>()).Returns(hero);
        factionsRepository.GetByIdAsync(faction.Id, Arg.Any<CancellationToken>()).Returns(faction);
        var handler = new UpdateHeroHandler(repository, factionsRepository);

        var result = await handler.HandleAsync(
            HeroTestData.CreateUpdateCommand(hero.Id.Value, faction.Id.Value) with
            {
                Name = string.Empty,
                Description = string.Empty,
                Attack = -1,
                MinimumDamage = 10,
                MaximumDamage = 1,
                Initiative = double.NaN
            },
            TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThanOrEqualTo(5);
        result.Errors.ShouldAllBe(error => error.Type == ErrorType.Validation);
        hero.Name.ShouldBeSameAs(originalName);
        hero.Stats.ShouldBeSameAs(originalStats);
        hero.FactionId.ShouldBe(faction.Id);
        await repository.DidNotReceiveWithAnyArgs()
            .UpdateAsync(null!, TestContext.Current.CancellationToken);
    }
}
