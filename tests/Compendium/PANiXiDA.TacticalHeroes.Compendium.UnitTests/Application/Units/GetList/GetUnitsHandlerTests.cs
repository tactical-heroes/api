using PANiXiDA.TacticalHeroes.Compendium.Application.Units.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Application.Units.GetList;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Application.Units.GetList;

public sealed class GetUnitsHandlerTests
{
    [Fact(DisplayName = "Units handler should return a repository page when repository succeeds")]
    public async Task HandleAsync_Should_ReturnPage_When_RepositorySucceeds()
    {
        var pagination = new PaginationParameters(1, 20);
        var unitsReadRepository = Substitute.For<IUnitsReadRepository>();
        var handler = new GetUnitsHandler(unitsReadRepository);

        var result = await handler.HandleAsync(
            new GetUnitsQuery(pagination),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await unitsReadRepository.Received(1).GetPagedAsync(
            pagination,
            TestContext.Current.CancellationToken);
    }
}
