using Microsoft.EntityFrameworkCore;
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
        persistedUnit.Stats.Health.ShouldBe(12);
        persistedUnit.Stats.MinimumDamage.ShouldBe(3);
        persistedUnit.Stats.MaximumDamage.ShouldBe(5);
        persistedUnit.Stats.Initiative.ShouldBe(10.5);
        persistedUnit.Stats.Shots.ShouldBe(12);
        persistedUnit.Stats.RangedAttackRange.ShouldBe(8);
        persistedUnit.Morale.Value.ShouldBe(2);
        persistedUnit.Luck.Value.ShouldBe(1);
        persistedUnit.FactionId.ShouldBe(faction.Id);
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

    [Fact(DisplayName = "UpdateAsync should persist unit changes when unit exists")]
    public async Task UpdateAsync_Should_PersistChanges_When_UnitExists()
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
            unitToUpdate.Update(new UnitAttributes
            {
                Name = "Marksman",
                Description = "An elite ranged unit.",
                CombatStats = new UnitCombatStatsInput
                {
                    Attack = 10,
                    Defense = 5,
                    Health = 14,
                    MinimumDamage = 4,
                    MaximumDamage = 7,
                    Initiative = 11.5,
                    Speed = 7,
                    Shots = 16,
                    RangedAttackRange = 10
                },
                Morale = 3,
                Luck = 2,
                FactionId = faction.Id.Value
            })
                .IsSuccess.ShouldBeTrue();

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
        persistedUnit.Stats.Shots.ShouldBe(16);
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
