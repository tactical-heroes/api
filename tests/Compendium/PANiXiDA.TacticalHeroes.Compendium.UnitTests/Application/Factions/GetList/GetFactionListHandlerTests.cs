using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetList;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Application.Factions.GetList;

public sealed class GetFactionListHandlerTests
{
    [Fact(DisplayName = "Factions handler should return a repository page when repository succeeds")]
    public async Task HandleAsync_Should_ReturnPage_When_RepositorySucceeds()
    {
        var pagination = new PaginationParameters(1, 20);
        var factionsReadRepository = Substitute.For<IFactionsReadRepository>();
        var handler = new GetFactionListHandler(factionsReadRepository);

        var result = await handler.HandleAsync(
            new GetFactionListQuery(pagination),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await factionsReadRepository.Received(1).GetPagedAsync(
            pagination,
            TestContext.Current.CancellationToken);
    }
}
