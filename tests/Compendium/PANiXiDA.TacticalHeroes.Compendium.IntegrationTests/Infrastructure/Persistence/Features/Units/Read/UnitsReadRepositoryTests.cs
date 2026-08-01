using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Compendium.Application.Units.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Units;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Units.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Compendium.IntegrationTests.Units;

namespace PANiXiDA.TacticalHeroes.Compendium.IntegrationTests.Infrastructure.Persistence.Features.Units.Read;

public sealed class UnitsReadRepositoryTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "GetDetailsByIdAsync should return unit details when unit exists")]
    public async Task GetDetailsByIdAsync_Should_ReturnDetails_When_UnitExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var faction = IntegrationTestData.CreateFaction();
        var unit = IntegrationTestData.CreateUnit(faction);
        await AddFactionAndUnitsAsync(
            faction,
            cancellationToken,
            unit);

        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider
            .GetRequiredService<IUnitsReadRepository>();
        var details = await repository.GetDetailsByIdAsync(
            unit.Id.Value,
            cancellationToken);

        details.ShouldNotBeNull();
        details.Name.ShouldBe("Archer");
        details.Attack.ShouldBe(8);
        details.Defense.ShouldBe(4);
        details.Health.ShouldBe(12);
        details.MinimumDamage.ShouldBe(3);
        details.MaximumDamage.ShouldBe(5);
        details.Initiative.ShouldBe(10.5);
        details.Speed.ShouldBe(6);
        details.Shots.ShouldBe(12);
        details.RangedAttackRange.ShouldBe(8);
        details.Morale.ShouldBe(2);
        details.Luck.ShouldBe(1);
        details.FactionId.ShouldBe(faction.Id.Value);
    }

    [Fact(DisplayName = "GetPagedAsync should return units sorted by name when units exist")]
    public async Task GetPagedAsync_Should_ReturnSortedPage_When_UnitsExist()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var faction = IntegrationTestData.CreateFaction();
        var archer = IntegrationTestData.CreateUnit(faction, "Archer");
        var marksman = IntegrationTestData.CreateUnit(faction, "Marksman");
        await AddFactionAndUnitsAsync(
            faction,
            cancellationToken,
            marksman,
            archer);

        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider
            .GetRequiredService<IUnitsReadRepository>();
        var page = await repository.GetPagedAsync(
            new PaginationParameters(1, 20),
            cancellationToken);

        page.TotalCount.ShouldBe(2);
        page.Items.Select(item => item.Name)
            .ShouldBe(["Archer", "Marksman"]);
        page.Items.ShouldAllBe(item => item.FactionId == faction.Id.Value);
    }

    [Fact(DisplayName = "ExistsByIdAsync should return true when unit exists")]
    public async Task ExistsByIdAsync_Should_ReturnTrue_When_UnitExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var faction = IntegrationTestData.CreateFaction();
        var unit = IntegrationTestData.CreateUnit(faction);
        await AddFactionAndUnitsAsync(
            faction,
            cancellationToken,
            unit);

        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider
            .GetRequiredService<IUnitsReadRepository>();

        (await repository.ExistsByIdAsync(unit.Id.Value, cancellationToken))
            .ShouldBeTrue();
    }

    [Fact(DisplayName = "AnyAsync should reflect whether units exist when called")]
    public async Task AnyAsync_Should_ReflectWhetherUnitsExist_When_Called()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        await using (var emptyScope = Fixture.CreateScope())
        {
            var emptyRepository = emptyScope.ServiceProvider
                .GetRequiredService<IUnitsReadRepository>();
            (await emptyRepository.AnyAsync(cancellationToken)).ShouldBeFalse();
        }

        var faction = IntegrationTestData.CreateFaction();
        await AddFactionAndUnitsAsync(
            faction,
            cancellationToken,
            IntegrationTestData.CreateUnit(faction));

        await using var populatedScope = Fixture.CreateScope();
        var populatedRepository = populatedScope.ServiceProvider
            .GetRequiredService<IUnitsReadRepository>();
        (await populatedRepository.AnyAsync(cancellationToken)).ShouldBeTrue();
    }

    private async Task AddFactionAndUnitsAsync(
        Faction faction,
        CancellationToken cancellationToken,
        params Unit[] units)
    {
        await using var scope = Fixture.CreateScope();
        var factionsRepository = scope.ServiceProvider
            .GetRequiredService<IFactionsRepository>();
        var unitsRepository = scope.ServiceProvider
            .GetRequiredService<IUnitsRepository>();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<CompendiumWriteDbContext>();

        await factionsRepository.AddAsync(faction, cancellationToken);

        foreach (var unit in units)
        {
            await unitsRepository.AddAsync(unit, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
