using PANiXiDA.TacticalHeroes.Compendium.Application.Units.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Application.Units.GetDetails;
using PANiXiDA.TacticalHeroes.Compendium.UnitTests.Units;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Application.Units.GetDetails;

public sealed class GetUnitDetailsHandlerTests
{
    [Fact(DisplayName = "Unit details handler should return an existing unit when unit exists")]
    public async Task HandleAsync_Should_ReturnUnit_When_UnitExists()
    {
        var readModel = UnitTestData.CreateDetailsReadModel(
            Guid.CreateVersion7());
        var repository = Substitute.For<IUnitsReadRepository>();
        repository.GetDetailsByIdAsync(
                readModel.Id,
                Arg.Any<CancellationToken>())
            .Returns(readModel);
        var handler = new GetUnitDetailsHandler(repository);

        var result = await handler.HandleAsync(
            new GetUnitDetailsQuery(readModel.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(readModel);
    }

    [Fact(DisplayName = "Unit details handler should return not found when unit does not exist")]
    public async Task HandleAsync_Should_ReturnNotFound_When_UnitDoesNotExist()
    {
        var repository = Substitute.For<IUnitsReadRepository>();
        repository.GetDetailsByIdAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>())
            .Returns((UnitDetailsReadModel?)null);
        var handler = new GetUnitDetailsHandler(repository);

        var result = await handler.HandleAsync(
            new GetUnitDetailsQuery(Guid.CreateVersion7()),
            TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(
            ErrorType.NotFound,
            "Unit was not found.");
    }
}
