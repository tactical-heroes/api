using PANiXiDA.TacticalHeroes.Compendium.Application.Units.Create;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Units;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Units.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.UnitTests.Units;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Application.Units.Create;

public sealed class CreateUnitHandlerTests
{
    [Fact(DisplayName = "Create unit handler should add a valid unit when faction exists")]
    public async Task HandleAsync_Should_AddUnit_When_FactionExists()
    {
        var faction = UnitTestData.CreateFaction();
        var unitsRepository = Substitute.For<IUnitsRepository>();
        var factionsRepository = Substitute.For<IFactionsRepository>();
        factionsRepository.GetByIdAsync(
            faction.Id,
            Arg.Any<CancellationToken>())
            .Returns(faction);
        var handler = new CreateUnitHandler(
            unitsRepository,
            factionsRepository);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            UnitTestData.CreateCommand(faction.Id.Value),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await unitsRepository.Received(1).AddAsync(
            Arg.Is<Unit>(unit =>
                unit.Id.Value == result.Value &&
                unit.Name.Value == "Archer" &&
                unit.Stats.Attack == 8 &&
                unit.RangedAttack.Shots == 12 &&
                unit.FactionId == faction.Id),
            cancellationToken);
    }

    [Fact(DisplayName = "Create unit handler should return not found when faction does not exist")]
    public async Task HandleAsync_Should_ReturnNotFound_When_FactionDoesNotExist()
    {
        var unitsRepository = Substitute.For<IUnitsRepository>();
        var factionsRepository = Substitute.For<IFactionsRepository>();
        factionsRepository.GetByIdAsync(
            Arg.Any<FactionId>(),
            Arg.Any<CancellationToken>())
            .Returns((Faction?)null);
        var handler = new CreateUnitHandler(
            unitsRepository,
            factionsRepository);

        var result = await handler.HandleAsync(
            UnitTestData.CreateCommand(Guid.CreateVersion7()),
            TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(
            ErrorType.NotFound,
            "Faction was not found.");
        await unitsRepository.DidNotReceiveWithAnyArgs()
            .AddAsync(null!, TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Create unit handler should reject invalid values without saving when command is invalid")]
    public async Task HandleAsync_Should_ReturnValidationFailuresWithoutSaving_When_CommandIsInvalid()
    {
        var faction = UnitTestData.CreateFaction();
        var repository = Substitute.For<IUnitsRepository>();
        var factionsRepository = Substitute.For<IFactionsRepository>();
        var handler = new CreateUnitHandler(repository, factionsRepository);

        var result = await handler.HandleAsync(
            UnitTestData.CreateCommand(faction.Id.Value) with
            {
                Name = string.Empty,
                Description = string.Empty,
                Attack = -1,
                MinimumDamage = 10,
                MaximumDamage = 1,
                Initiative = double.NaN,
                Shots = 0,
                RangedAttackRange = null
            },
            TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThanOrEqualTo(7);
        result.Errors.ShouldAllBe(error => error.Type == ErrorType.Validation);
        await repository.DidNotReceiveWithAnyArgs()
            .AddAsync(null!, TestContext.Current.CancellationToken);
    }
}
