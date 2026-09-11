using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.GetDetails;
using PANiXiDA.TacticalHeroes.Compendium.UnitTests.Heroes;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Application.Heroes.GetDetails;

public sealed class GetHeroDetailsHandlerTests
{
    [Fact(DisplayName = "Hero details handler should return an existing hero when hero exists")]
    public async Task HandleAsync_Should_ReturnHero_When_HeroExists()
    {
        var readModel = HeroTestData.CreateDetailsReadModel(
            Guid.CreateVersion7());
        var repository = Substitute.For<IHeroesReadRepository>();
        repository.GetDetailsByIdAsync(
                readModel.Id,
                Arg.Any<CancellationToken>())
            .Returns(readModel);
        var handler = new GetHeroDetailsHandler(repository);

        var result = await handler.HandleAsync(
            new GetHeroDetailsQuery(readModel.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(readModel);
    }

    [Fact(DisplayName = "Hero details handler should return not found when hero does not exist")]
    public async Task HandleAsync_Should_ReturnNotFound_When_HeroDoesNotExist()
    {
        var repository = Substitute.For<IHeroesReadRepository>();
        repository.GetDetailsByIdAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>())
            .Returns((HeroDetailsReadModel?)null);
        var handler = new GetHeroDetailsHandler(repository);

        var result = await handler.HandleAsync(
            new GetHeroDetailsQuery(Guid.CreateVersion7()),
            TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(
            ErrorType.NotFound,
            "Hero was not found.");
    }
}
