using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.GetList;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Application.Heroes.GetList;

public sealed class GetHeroesHandlerTests
{
    [Fact(DisplayName = "Heroes handler should return a repository page when repository succeeds")]
    public async Task HandleAsync_Should_ReturnPage_When_RepositorySucceeds()
    {
        var pagination = new PaginationParameters(1, 20);
        var heroesReadRepository = Substitute.For<IHeroesReadRepository>();
        var handler = new GetHeroesHandler(heroesReadRepository);

        var result = await handler.HandleAsync(
            new GetHeroesQuery(pagination),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await heroesReadRepository.Received(1).GetPageAsync(
            pagination,
            TestContext.Current.CancellationToken);
    }
}
