using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.ValueObjects;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Compendium.IntegrationTests.Infrastructure.Persistence.Features.Factions.Write;

public sealed class FactionsRepositoryTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "Add should persist a valid faction when faction is valid")]
    public async Task Add_Should_PersistFaction_When_FactionIsValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var faction = CreateFaction();

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IFactionsRepository>();
            var dbContext = scope.ServiceProvider.GetRequiredService<CompendiumWriteDbContext>();

            repository.Add(faction);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        await using var verificationScope = Fixture.CreateScope();
        var verificationDbContext =
            verificationScope.ServiceProvider.GetRequiredService<CompendiumWriteDbContext>();
        var persistedFaction = await verificationDbContext.Set<Faction>()
            .AsNoTracking()
            .SingleAsync(item => item.Id == faction.Id, cancellationToken);

        persistedFaction.Name.Value.ShouldBe("Northern Alliance");
        persistedFaction.Description.Value.ShouldBe("Defenders of the north.");
    }

    [Fact(DisplayName = "GetByIdAsync should return an existing faction when faction exists")]
    public async Task GetByIdAsync_Should_ReturnFaction_When_FactionExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var faction = CreateFaction();
        await AddFactionAsync(faction, cancellationToken);

        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFactionsRepository>();
        var persistedFaction = await repository.GetByIdAsync(
            faction.Id,
            cancellationToken);

        persistedFaction.ShouldNotBeNull();
        persistedFaction.Name.Value.ShouldBe("Northern Alliance");
        persistedFaction.Description.Value.ShouldBe("Defenders of the north.");
    }

    [Fact(DisplayName = "Update should persist faction changes when faction exists")]
    public async Task Update_Should_PersistChanges_When_FactionExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var faction = CreateFaction();
        await AddFactionAsync(faction, cancellationToken);

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IFactionsRepository>();
            var dbContext = scope.ServiceProvider.GetRequiredService<CompendiumWriteDbContext>();
            var factionToUpdate = await repository.GetByIdAsync(
                faction.Id,
                cancellationToken);

            factionToUpdate.ShouldNotBeNull();
            factionToUpdate.Update(
                name: FactionName.Create(value: "Southern Alliance").Value,
                description: FactionDescription.Create(value: "Defenders of the south.").Value);

            repository.Update(factionToUpdate);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        await using var verificationScope = Fixture.CreateScope();
        var verificationDbContext =
            verificationScope.ServiceProvider.GetRequiredService<CompendiumWriteDbContext>();
        var persistedFaction = await verificationDbContext.Set<Faction>()
            .AsNoTracking()
            .SingleAsync(
                item => item.Id == faction.Id,
                cancellationToken);

        persistedFaction.Name.Value.ShouldBe("Southern Alliance");
        persistedFaction.Description.Value.ShouldBe("Defenders of the south.");
    }

    [Fact(DisplayName = "Delete should soft delete an existing faction when faction exists")]
    public async Task Delete_Should_SoftDeleteFaction_When_FactionExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var faction = CreateFaction();
        await AddFactionAsync(faction, cancellationToken);

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IFactionsRepository>();
            var dbContext = scope.ServiceProvider.GetRequiredService<CompendiumWriteDbContext>();
            var persistedFaction = await repository.GetByIdAsync(
                faction.Id,
                cancellationToken);

            persistedFaction.ShouldNotBeNull();
            repository.Delete(persistedFaction);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IFactionsRepository>();

            (await repository.GetByIdAsync(faction.Id, cancellationToken))
                .ShouldBeNull();
        }
    }

    private static Faction CreateFaction()
    {
        return Faction.Create(
            name: FactionName.Create(value: "Northern Alliance").Value,
            description: FactionDescription.Create(value: "Defenders of the north.").Value);
    }

    private async Task AddFactionAsync(
        Faction faction,
        CancellationToken cancellationToken)
    {
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFactionsRepository>();
        var dbContext = scope.ServiceProvider.GetRequiredService<CompendiumWriteDbContext>();

        repository.Add(faction);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
