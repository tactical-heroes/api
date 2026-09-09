using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Units;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Units.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Units.ValueObjects;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Compendium.IntegrationTests.Units;

namespace PANiXiDA.TacticalHeroes.Compendium.IntegrationTests.Infrastructure.Persistence.Features.Units.Write;

public sealed class UnitsRepositoryTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "AddAsync should persist a valid unit when unit and faction are valid")]
    public async Task AddAsync_Should_PersistUnit_When_UnitAndFactionAreValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var faction = IntegrationTestData.CreateFaction();
        var unit = IntegrationTestData.CreateUnit(faction);

        await AddFactionAndUnitsAsync(
            faction,
            cancellationToken,
            unit);

        await using var verificationScope = Fixture.CreateScope();
        var dbContext = verificationScope.ServiceProvider
            .GetRequiredService<CompendiumWriteDbContext>();
        var persistedUnit = await dbContext.Set<Unit>()
            .AsNoTracking()
            .SingleAsync(item => item.Id == unit.Id, cancellationToken);

        persistedUnit.Name.Value.ShouldBe("Archer");
        persistedUnit.Stats.Attack.ShouldBe(8);
        persistedUnit.Stats.Defense.ShouldBe(4);
        persistedUnit.Stats.Health.ShouldBe(12);
        persistedUnit.Stats.MinimumDamage.ShouldBe(3);
        persistedUnit.Stats.MaximumDamage.ShouldBe(5);
        persistedUnit.Stats.Initiative.ShouldBe(10.5);
        persistedUnit.Stats.Speed.ShouldBe(6);
        persistedUnit.RangedAttack.Shots.ShouldBe(12);
        persistedUnit.RangedAttack.RangedAttackRange.ShouldBe(8);
        persistedUnit.Morale.Value.ShouldBe(2);
        persistedUnit.Luck.Value.ShouldBe(1);
        persistedUnit.FactionId.ShouldBe(faction.Id);
    }

    [Fact(DisplayName = "MigrateAsync should preserve ranged attack values when columns are renamed")]
    public async Task MigrateAsync_Should_PreserveRangedAttack_When_ColumnsAreRenamed()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var faction = IntegrationTestData.CreateFaction();
        var unit = IntegrationTestData.CreateUnit(faction);
        await AddFactionAndUnitsAsync(faction, cancellationToken, unit);

        await using var scope = Fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CompendiumWriteDbContext>();
        var migrator = dbContext.GetService<IMigrator>();

        try
        {
            await migrator.MigrateAsync("20260801172140_AddHeroes", cancellationToken);
        }
        finally
        {
            await migrator.MigrateAsync(cancellationToken: cancellationToken);
        }

        var persistedUnit = await dbContext.Set<Unit>()
            .AsNoTracking()
            .SingleAsync(item => item.Id == unit.Id, cancellationToken);

        persistedUnit.RangedAttack.Shots.ShouldBe(12);
        persistedUnit.RangedAttack.RangedAttackRange.ShouldBe(8);
    }

    [Fact(DisplayName = "GetByIdAsync should return an existing unit when unit exists")]
    public async Task GetByIdAsync_Should_ReturnUnit_When_UnitExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var faction = IntegrationTestData.CreateFaction();
        var unit = IntegrationTestData.CreateUnit(faction);
        await AddFactionAndUnitsAsync(
            faction,
            cancellationToken,
            unit);

        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUnitsRepository>();
        var persistedUnit = await repository.GetByIdAsync(
            unit.Id,
            cancellationToken);

        persistedUnit.ShouldNotBeNull();
        persistedUnit.Name.Value.ShouldBe("Archer");
        persistedUnit.FactionId.ShouldBe(faction.Id);
    }

    [Theory(DisplayName = "UpdateAsync should persist unit changes when unit exists")]
    [InlineData(16, 10)]
    [InlineData(null, null)]
    public async Task UpdateAsync_Should_PersistChanges_When_UnitExists(int? shots, int? rangedAttackRange)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var faction = IntegrationTestData.CreateFaction();
        var unit = IntegrationTestData.CreateUnit(faction);
        await AddFactionAndUnitsAsync(
            faction,
            cancellationToken,
            unit);

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider
                .GetRequiredService<IUnitsRepository>();
            var dbContext = scope.ServiceProvider
                .GetRequiredService<CompendiumWriteDbContext>();
            var unitToUpdate = await repository.GetByIdAsync(
                unit.Id,
                cancellationToken);

            unitToUpdate.ShouldNotBeNull();
            unitToUpdate.Update(
            name: UnitName.Create(value: "Marksman").Value,
            description: UnitDescription.Create(value: "An elite ranged unit.").Value,
            stats: UnitCombatStats.Create(
                attack: 10,
                defense: 5,
                health: 14,
                minimumDamage: 4,
                maximumDamage: 7,
                initiative: 11.5,
                speed: 7).Value,
            rangedAttack: UnitRangedAttack.Create(
                shots: shots,
                rangedAttackRange: rangedAttackRange).Value,
            morale: UnitMorale.Create(value: 3).Value,
            luck: UnitLuck.Create(value: 2).Value,
            factionId: faction.Id);

            await repository.UpdateAsync(unitToUpdate, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        await using var verificationScope = Fixture.CreateScope();
        var verificationDbContext = verificationScope.ServiceProvider
            .GetRequiredService<CompendiumWriteDbContext>();
        var persistedUnit = await verificationDbContext.Set<Unit>()
            .AsNoTracking()
            .SingleAsync(item => item.Id == unit.Id, cancellationToken);

        persistedUnit.Name.Value.ShouldBe("Marksman");
        persistedUnit.Stats.Attack.ShouldBe(10);
        persistedUnit.Stats.MaximumDamage.ShouldBe(7);
        persistedUnit.RangedAttack.ShouldNotBeNull();
        persistedUnit.RangedAttack.Shots.ShouldBe(shots);
        persistedUnit.RangedAttack.RangedAttackRange.ShouldBe(rangedAttackRange);
        persistedUnit.Morale.Value.ShouldBe(3);
    }

    [Fact(DisplayName = "DeleteAsync should soft delete an existing unit when unit exists")]
    public async Task DeleteAsync_Should_SoftDeleteUnit_When_UnitExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var faction = IntegrationTestData.CreateFaction();
        var unit = IntegrationTestData.CreateUnit(faction);
        await AddFactionAndUnitsAsync(
            faction,
            cancellationToken,
            unit);

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider
                .GetRequiredService<IUnitsRepository>();
            var dbContext = scope.ServiceProvider
                .GetRequiredService<CompendiumWriteDbContext>();
            var persistedUnit = await repository.GetByIdAsync(
                unit.Id,
                cancellationToken);

            persistedUnit.ShouldNotBeNull();
            await repository.DeleteAsync(persistedUnit, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        await using var verificationScope = Fixture.CreateScope();
        var verificationRepository = verificationScope.ServiceProvider
            .GetRequiredService<IUnitsRepository>();
        (await verificationRepository.GetByIdAsync(
                unit.Id,
                cancellationToken))
            .ShouldBeNull();
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
