using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Compendium.IntegrationTests.Infrastructure.Persistence.Features.Factions.Write;

public sealed class FactionsRepositoryTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "Factions repository should persist update and soft delete faction state")]
    public async Task Repository_Should_PersistUpdateAndDelete_When_FactionIsValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var faction = Faction.Create(
            "Northern Alliance",
            "Defenders of the north.").Value;

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IFactionsRepository>();
            var dbContext = scope.ServiceProvider.GetRequiredService<CompendiumWriteDbContext>();

            await repository.AddAsync(faction, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IFactionsRepository>();
            var dbContext = scope.ServiceProvider.GetRequiredService<CompendiumWriteDbContext>();
            var persistedFaction = await repository.GetByIdAsync(
                faction.Id,
                cancellationToken);

            persistedFaction.ShouldNotBeNull();
            persistedFaction.UpdateDetails(
                    "Southern Alliance",
                    "Defenders of the south.")
                .IsSuccess.ShouldBeTrue();

            await repository.UpdateAsync(persistedFaction, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        await using (var scope = Fixture.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<CompendiumWriteDbContext>();
            var persistedFaction = await dbContext.Set<Faction>()
                .AsNoTracking()
                .SingleAsync(
                    item => item.Id == faction.Id,
                    cancellationToken);

            persistedFaction.Name.Value.ShouldBe("Southern Alliance");
            persistedFaction.Description.Value.ShouldBe("Defenders of the south.");
        }

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
}
