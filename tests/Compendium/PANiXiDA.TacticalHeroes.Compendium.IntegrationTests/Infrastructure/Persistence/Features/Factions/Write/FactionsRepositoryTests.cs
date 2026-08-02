using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Compendium.IntegrationTests.Infrastructure.Persistence.Features.Factions.Write;

public sealed class FactionsRepositoryTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "AddAsync should persist a valid faction when faction is valid")]
    public async Task AddAsync_Should_PersistFaction_When_FactionIsValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var faction = CreateFaction();

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IFactionsRepository>();
            var dbContext = scope.ServiceProvider.GetRequiredService<CompendiumWriteDbContext>();

            await repository.AddAsync(faction, cancellationToken);
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

    [Fact(DisplayName = "UpdateAsync should persist faction changes when faction exists")]
    public async Task UpdateAsync_Should_PersistChanges_When_FactionExists()
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
                    "Southern Alliance",
                    "Defenders of the south.")
                .IsSuccess.ShouldBeTrue();

            await repository.UpdateAsync(factionToUpdate, cancellationToken);
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

    [Fact(DisplayName = "DeleteAsync should soft delete an existing faction when faction exists")]
    public async Task DeleteAsync_Should_SoftDeleteFaction_When_FactionExists()
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
            await repository.DeleteAsync(persistedFaction, cancellationToken);
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
            "Northern Alliance",
            "Defenders of the north.").Value;
    }

    private async Task AddFactionAsync(
        Faction faction,
        CancellationToken cancellationToken)
    {
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFactionsRepository>();
        var dbContext = scope.ServiceProvider.GetRequiredService<CompendiumWriteDbContext>();

        await repository.AddAsync(faction, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
