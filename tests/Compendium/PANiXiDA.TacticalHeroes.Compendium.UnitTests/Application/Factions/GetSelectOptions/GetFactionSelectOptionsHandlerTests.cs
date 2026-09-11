using PANiXiDA.Core.Application.Querying.Limiting;
using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Common.Filters;
using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetSelectOptions;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Application.Factions.GetSelectOptions;

public sealed class GetFactionSelectOptionsHandlerTests
{
    [Fact(DisplayName = "HandleAsync should return repository options when repository succeeds")]
    public async Task HandleAsync_Should_ReturnRepositoryOptions_When_RepositorySucceeds()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = Substitute.For<IFactionsReadRepository>();
        var filter = new FactionsFilter("north");
        List<FactionSelectOptionReadModel> options = [new(Guid.NewGuid(), "Northern Alliance")];
        repository.GetSelectOptionsAsync(filter, new LimitParameters(10), cancellationToken).Returns(options);
        var handler = new GetFactionSelectOptionsHandler(repository);

        var result = await handler.HandleAsync(new GetFactionSelectOptionsQuery(filter, new LimitParameters(10)), cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(options);
        await repository.Received(1).GetSelectOptionsAsync(filter, new LimitParameters(10), cancellationToken);
    }
}
