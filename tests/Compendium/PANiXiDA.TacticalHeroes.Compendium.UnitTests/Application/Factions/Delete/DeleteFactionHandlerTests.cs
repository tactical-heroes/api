using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Delete;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Application.Factions.Delete;

public sealed class DeleteFactionHandlerTests
{
    [Fact(DisplayName = "Delete faction handler should delete an existing faction")]
    public async Task HandleAsync_Should_DeleteFaction_When_FactionExists()
    {
        var faction = Faction.Create(
            "Northern Alliance",
            "Defenders of the north.").Value;
        var repository = Substitute.For<IFactionsRepository>();
        repository.GetByIdAsync(faction.Id, Arg.Any<CancellationToken>())
            .Returns(faction);
        var handler = new DeleteFactionHandler(repository);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            new DeleteFactionCommand(faction.Id.Value),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await repository.Received(1).DeleteAsync(faction, cancellationToken);
    }

    [Fact(DisplayName = "Delete faction handler should return not found for a missing faction")]
    public async Task HandleAsync_Should_ReturnNotFound_When_FactionDoesNotExist()
    {
        var repository = Substitute.For<IFactionsRepository>();
        repository.GetByIdAsync(
                Arg.Any<FactionId>(),
                Arg.Any<CancellationToken>())
            .Returns((Faction?)null);
        var handler = new DeleteFactionHandler(repository);

        var result = await handler.HandleAsync(
            new DeleteFactionCommand(Guid.CreateVersion7()),
            TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(
            ErrorType.NotFound,
            "Faction was not found.");
    }
}
