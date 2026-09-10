using PANiXiDA.TacticalHeroes.Compendium.Application.Units.Update;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Units;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Units.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.UnitTests.Units;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Application.Units.Update;

public sealed class UpdateUnitHandlerTests
{
    [Fact(DisplayName = "Update unit handler should update an existing unit when unit and faction exist")]
    public async Task HandleAsync_Should_UpdateUnit_When_UnitAndFactionExist()
    {
        var faction = UnitTestData.CreateFaction();
        var unit = UnitTestData.CreateUnit(faction);
        var unitsRepository = Substitute.For<IUnitsRepository>();
        unitsRepository.GetByIdAsync(unit.Id, Arg.Any<CancellationToken>())
            .Returns(unit);
        var factionsRepository = Substitute.For<IFactionsRepository>();
        factionsRepository.GetByIdAsync(
            faction.Id,
            Arg.Any<CancellationToken>())
            .Returns(faction);
        var handler = new UpdateUnitHandler(
            unitsRepository,
            factionsRepository);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            UnitTestData.CreateUpdateCommand(
                unit.Id.Value,
                faction.Id.Value),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        unit.Name.Value.ShouldBe("Marksman");
        unit.Stats.Attack.ShouldBe(10);
        unit.RangedAttack.Shots.ShouldBe(16);
        unit.Morale.Value.ShouldBe(3);
        await unitsRepository.Received(1).UpdateAsync(unit, cancellationToken);
    }

    [Fact(DisplayName = "Update unit handler should return not found when unit does not exist")]
    public async Task HandleAsync_Should_ReturnNotFound_When_UnitDoesNotExist()
    {
        var unitsRepository = Substitute.For<IUnitsRepository>();
        unitsRepository.GetByIdAsync(
            Arg.Any<UnitId>(),
            Arg.Any<CancellationToken>())
            .Returns((Unit?)null);
        var handler = new UpdateUnitHandler(
            unitsRepository,
            Substitute.For<IFactionsRepository>());

        var result = await handler.HandleAsync(
            UnitTestData.CreateUpdateCommand(
                Guid.CreateVersion7(),
                Guid.CreateVersion7()),
            TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(
            ErrorType.NotFound,
            "Unit was not found.");
    }

    [Fact(DisplayName = "Update unit handler should return not found when faction does not exist")]
    public async Task HandleAsync_Should_ReturnNotFound_When_FactionDoesNotExist()
    {
        var faction = UnitTestData.CreateFaction();
        var unit = UnitTestData.CreateUnit(faction);
        var unitsRepository = Substitute.For<IUnitsRepository>();
        unitsRepository.GetByIdAsync(unit.Id, Arg.Any<CancellationToken>())
            .Returns(unit);
        var factionsRepository = Substitute.For<IFactionsRepository>();
        factionsRepository.GetByIdAsync(
            Arg.Any<FactionId>(),
            Arg.Any<CancellationToken>())
            .Returns((Faction?)null);
        var handler = new UpdateUnitHandler(
            unitsRepository,
            factionsRepository);

        var result = await handler.HandleAsync(
            UnitTestData.CreateUpdateCommand(
                unit.Id.Value,
                Guid.CreateVersion7()),
            TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(
            ErrorType.NotFound,
            "Faction was not found.");
        unit.Name.Value.ShouldBe("Archer");
        await unitsRepository.DidNotReceiveWithAnyArgs()
            .UpdateAsync(null!, TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Update unit handler should reject invalid values without saving when command is invalid")]
    public async Task HandleAsync_Should_ReturnValidationFailuresWithoutSaving_When_CommandIsInvalid()
    {
        var faction = UnitTestData.CreateFaction();
        var repository = Substitute.For<IUnitsRepository>();
        var factionsRepository = Substitute.For<IFactionsRepository>();
        var unit = UnitTestData.CreateUnit(faction);
        var originalName = unit.Name;
        var originalStats = unit.Stats;
        var originalRangedAttack = unit.RangedAttack;
        repository.GetByIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns(unit);
        factionsRepository.GetByIdAsync(faction.Id, Arg.Any<CancellationToken>()).Returns(faction);
        var handler = new UpdateUnitHandler(repository, factionsRepository);

        var result = await handler.HandleAsync(
            UnitTestData.CreateUpdateCommand(unit.Id.Value, faction.Id.Value) with
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
        unit.Name.ShouldBeSameAs(originalName);
        unit.Stats.ShouldBeSameAs(originalStats);
        unit.RangedAttack.ShouldBeSameAs(originalRangedAttack);
        unit.FactionId.ShouldBe(faction.Id);
        await repository.DidNotReceiveWithAnyArgs()
            .UpdateAsync(null!, TestContext.Current.CancellationToken);
    }
}
