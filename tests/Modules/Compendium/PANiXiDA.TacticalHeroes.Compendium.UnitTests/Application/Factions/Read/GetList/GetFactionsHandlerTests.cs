using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Read.GetList;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Application.Factions.Read.GetList;

public sealed class GetFactionsHandlerTests
{
    [Fact(DisplayName = "Factions handler should return a repository page when repository succeeds")]
    public async Task HandleAsync_Should_ReturnPage_When_RepositorySucceeds()
    {
        var pagination = new PaginationParameters(1, 20);
        var sorting = SortingParameters.None;
        var factionsReadRepository = Substitute.For<IFactionsReadRepository>();
        var handler = new GetFactionsHandler(factionsReadRepository);

        var result = await handler.HandleAsync(
            new GetFactionsQuery(pagination, sorting),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await factionsReadRepository.Received(1).GetPageAsync(
            pagination,
            sorting,
            TestContext.Current.CancellationToken);
    }
}
