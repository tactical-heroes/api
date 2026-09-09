using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Create;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.Abstractions;

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
    [Fact(DisplayName = "Create faction handler should reject invalid details without saving when command is invalid")]
    public async Task HandleAsync_Should_ReturnValidationFailuresWithoutSaving_When_CommandIsInvalid()
    {
        var repository = Substitute.For<IFactionsRepository>();
        var handler = new CreateFactionHandler(repository);

        var result = await handler.HandleAsync(
            new CreateFactionCommand(string.Empty, string.Empty),
            TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.Count.ShouldBe(2);
        await repository.DidNotReceiveWithAnyArgs()
            .AddAsync(null!, TestContext.Current.CancellationToken);
    }

}
