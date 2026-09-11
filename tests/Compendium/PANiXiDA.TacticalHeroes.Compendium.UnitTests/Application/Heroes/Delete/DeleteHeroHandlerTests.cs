using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Delete;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.UnitTests.Heroes;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Application.Heroes.Delete;

public sealed class DeleteHeroHandlerTests
{
    [Fact(DisplayName = "Delete hero handler should delete an existing hero when hero exists")]
    public async Task HandleAsync_Should_DeleteHero_When_HeroExists()
    {
        var faction = HeroTestData.CreateFaction();
        var hero = HeroTestData.CreateHero(faction);
        var repository = Substitute.For<IHeroesRepository>();
        repository.GetByIdAsync(hero.Id, Arg.Any<CancellationToken>())
            .Returns(hero);
        var handler = new DeleteHeroHandler(repository);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            new DeleteHeroCommand(hero.Id.Value),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await repository.Received(1).DeleteAsync(hero, cancellationToken);
    }

    [Fact(DisplayName = "Delete hero handler should return not found when hero does not exist")]
    public async Task HandleAsync_Should_ReturnNotFound_When_HeroDoesNotExist()
    {
        var repository = Substitute.For<IHeroesRepository>();
        repository.GetByIdAsync(
                Arg.Any<HeroId>(),
                Arg.Any<CancellationToken>())
            .Returns((Hero?)null);
        var handler = new DeleteHeroHandler(repository);

        var result = await handler.HandleAsync(
            new DeleteHeroCommand(Guid.CreateVersion7()),
            TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(
            ErrorType.NotFound,
            "Hero was not found.");
    }
}
