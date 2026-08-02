using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Update;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Application.Factions.Update;

public sealed class UpdateFactionHandlerTests
{
    [Fact(DisplayName = "Update faction handler should update an existing faction when faction exists")]
    public async Task HandleAsync_Should_UpdateFaction_When_FactionExists()
    {
        var faction = Faction.Create(
            "Northern Alliance",
            "Defenders of the north.").Value;
        var repository = Substitute.For<IFactionsRepository>();
        repository.GetByIdAsync(faction.Id, Arg.Any<CancellationToken>())
            .Returns(faction);
        var handler = new UpdateFactionHandler(repository);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            new UpdateFactionCommand(
                faction.Id.Value,
                "Southern Alliance",
                "Defenders of the south."),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        faction.Name.Value.ShouldBe("Southern Alliance");
        faction.Description.Value.ShouldBe("Defenders of the south.");
        await repository.Received(1).UpdateAsync(faction, cancellationToken);
    }

    [Fact(DisplayName = "Update faction handler should return not found for a missing faction when faction does not exist")]
    public async Task HandleAsync_Should_ReturnNotFound_When_FactionDoesNotExist()
    {
        var repository = Substitute.For<IFactionsRepository>();
        repository.GetByIdAsync(
                Arg.Any<FactionId>(),
                Arg.Any<CancellationToken>())
            .Returns((Faction?)null);
        var handler = new UpdateFactionHandler(repository);

        var result = await handler.HandleAsync(
            new UpdateFactionCommand(
                Guid.CreateVersion7(),
                "Northern Alliance",
                "Defenders of the north."),
            TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(
            ErrorType.NotFound,
            "Faction was not found.");
    }
}
