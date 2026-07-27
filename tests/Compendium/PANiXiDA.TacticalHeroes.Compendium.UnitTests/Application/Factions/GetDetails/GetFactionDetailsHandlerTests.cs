using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetDetails;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Application.Factions.GetDetails;

public sealed class GetFactionDetailsHandlerTests
{
    [Fact(DisplayName = "Faction details handler should return an existing faction")]
    public async Task HandleAsync_Should_ReturnFaction_When_FactionExists()
    {
        var factionId = Guid.CreateVersion7();
        var readModel = new FactionDetailsReadModel(
            factionId,
            "Northern Alliance",
            "Defenders of the north.");
        var repository = Substitute.For<IFactionsReadRepository>();
        repository.GetDetailsByIdAsync(
                factionId,
                Arg.Any<CancellationToken>())
            .Returns(readModel);
        var handler = new GetFactionDetailsHandler(repository);

        var result = await handler.HandleAsync(
            new GetFactionDetailsQuery(factionId),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(readModel);
    }

    [Fact(DisplayName = "Faction details handler should return not found for a missing faction")]
    public async Task HandleAsync_Should_ReturnNotFound_When_FactionDoesNotExist()
    {
        var repository = Substitute.For<IFactionsReadRepository>();
        repository.GetDetailsByIdAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>())
            .Returns((FactionDetailsReadModel?)null);
        var handler = new GetFactionDetailsHandler(repository);

        var result = await handler.HandleAsync(
            new GetFactionDetailsQuery(Guid.CreateVersion7()),
            TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(
            ErrorType.NotFound,
            "Faction was not found.");
    }
}
