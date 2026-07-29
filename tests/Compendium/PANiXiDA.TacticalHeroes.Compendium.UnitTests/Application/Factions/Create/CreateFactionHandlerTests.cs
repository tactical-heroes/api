using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Create;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Application.Factions.Create;

public sealed class CreateFactionHandlerTests
{
    [Fact(DisplayName = "Create faction handler should add a valid faction when command is valid")]
    public async Task HandleAsync_Should_AddFaction_When_CommandIsValid()
    {
        var repository = Substitute.For<IFactionsRepository>();
        var handler = new CreateFactionHandler(repository);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            new CreateFactionCommand(
                "Northern Alliance",
                "Defenders of the north."),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await repository.Received(1).AddAsync(
            Arg.Is<Faction>(faction =>
                faction.Id.Value == result.Value &&
                faction.Name.Value == "Northern Alliance" &&
                faction.Description.Value == "Defenders of the north."),
            cancellationToken);
    }
}
