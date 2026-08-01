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
}
