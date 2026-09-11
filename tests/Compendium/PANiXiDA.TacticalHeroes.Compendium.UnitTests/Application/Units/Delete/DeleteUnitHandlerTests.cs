using PANiXiDA.TacticalHeroes.Compendium.Application.Units.Delete;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Units;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Units.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.UnitTests.Units;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Application.Units.Delete;

public sealed class DeleteUnitHandlerTests
{
    [Fact(DisplayName = "Delete unit handler should delete an existing unit when unit exists")]
    public async Task HandleAsync_Should_DeleteUnit_When_UnitExists()
    {
        var faction = UnitTestData.CreateFaction();
        var unit = UnitTestData.CreateUnit(faction);
        var repository = Substitute.For<IUnitsRepository>();
        repository.GetByIdAsync(unit.Id, Arg.Any<CancellationToken>())
            .Returns(unit);
        var handler = new DeleteUnitHandler(repository);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            new DeleteUnitCommand(unit.Id.Value),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await repository.Received(1).DeleteAsync(unit, cancellationToken);
    }

    [Fact(DisplayName = "Delete unit handler should return not found when unit does not exist")]
    public async Task HandleAsync_Should_ReturnNotFound_When_UnitDoesNotExist()
    {
        var repository = Substitute.For<IUnitsRepository>();
        repository.GetByIdAsync(
                Arg.Any<UnitId>(),
                Arg.Any<CancellationToken>())
            .Returns((Unit?)null);
        var handler = new DeleteUnitHandler(repository);

        var result = await handler.HandleAsync(
            new DeleteUnitCommand(Guid.CreateVersion7()),
            TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(
            ErrorType.NotFound,
            "Unit was not found.");
    }
}
