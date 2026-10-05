using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Create;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.UnitTests.Heroes;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Application.Heroes.Create;

public sealed class CreateHeroHandlerTests
{
    [Fact(DisplayName = "Create hero handler should add a valid hero when faction exists")]
    public async Task HandleAsync_Should_AddHero_When_FactionExists()
    {
        var faction = HeroTestData.CreateFaction();
        var heroesRepository = Substitute.For<IHeroesRepository>();
        var factionsRepository = Substitute.For<IFactionsRepository>();
        factionsRepository.GetByIdAsync(
                faction.Id,
                Arg.Any<CancellationToken>())
            .Returns(faction);
        var handler = new CreateHeroHandler(
            heroesRepository,
            factionsRepository);
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await handler.HandleAsync(
            HeroTestData.CreateCommand(faction.Id.Value),
            cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await heroesRepository.Received(1).AddAsync(
            Arg.Is<Hero>(hero =>
                hero.Id.Value == result.Value &&
                hero.Name.Value == "Orrin" &&
                hero.Stats.Attack == 8 &&
                hero.Stats.MaximumDamage == 7 &&
                hero.FactionId == faction.Id),
            cancellationToken);
    }

    [Fact(DisplayName = "Create hero handler should return not found when faction does not exist")]
    public async Task HandleAsync_Should_ReturnNotFound_When_FactionDoesNotExist()
    {
        var heroesRepository = Substitute.For<IHeroesRepository>();
        var factionsRepository = Substitute.For<IFactionsRepository>();
        factionsRepository.GetByIdAsync(
                Arg.Any<FactionId>(),
                Arg.Any<CancellationToken>())
            .Returns((Faction?)null);
        var handler = new CreateHeroHandler(
            heroesRepository,
            factionsRepository);

        var result = await handler.HandleAsync(
            HeroTestData.CreateCommand(Guid.CreateVersion7()),
            TestContext.Current.CancellationToken);

        result.ShouldHaveSingleError(
            ErrorType.NotFound,
            "Faction was not found.");
        await heroesRepository.DidNotReceiveWithAnyArgs()
            .AddAsync(null!, TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Create hero handler should reject invalid values without saving when command is invalid")]
    public async Task HandleAsync_Should_ReturnValidationFailuresWithoutSaving_When_CommandIsInvalid()
    {
        var faction = HeroTestData.CreateFaction();
        var repository = Substitute.For<IHeroesRepository>();
        var factionsRepository = Substitute.For<IFactionsRepository>();
        var handler = new CreateHeroHandler(repository, factionsRepository);

        var result = await handler.HandleAsync(
            HeroTestData.CreateCommand(faction.Id.Value) with
            {
                Name = string.Empty,
                Description = string.Empty,
                Attack = -1,
                MinimumDamage = 10,
                MaximumDamage = 1,
                Initiative = double.NaN
            },
            TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThanOrEqualTo(5);
        result.Errors.ShouldAllBe(error => error.Type == ErrorType.Validation);
        await repository.DidNotReceiveWithAnyArgs()
            .AddAsync(null!, TestContext.Current.CancellationToken);
    }
}
