using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes.ValueObjects;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Compendium.IntegrationTests.Heroes;

namespace PANiXiDA.TacticalHeroes.Compendium.IntegrationTests.Infrastructure.Persistence.Features.Heroes.Write;

public sealed class HeroesRepositoryTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "AddAsync should persist a valid hero when hero and faction are valid")]
    public async Task AddAsync_Should_PersistHero_When_HeroAndFactionAreValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var faction = IntegrationTestData.CreateFaction();
        var hero = IntegrationTestData.CreateHero(faction);

        await AddFactionAndHeroesAsync(
            faction,
            cancellationToken,
            hero);

        await using var verificationScope = Fixture.CreateScope();
        var dbContext = verificationScope.ServiceProvider
            .GetRequiredService<CompendiumWriteDbContext>();
        var persistedHero = await dbContext.Set<Hero>()
            .AsNoTracking()
            .SingleAsync(item => item.Id == hero.Id, cancellationToken);

        persistedHero.Name.Value.ShouldBe("Orrin");
        persistedHero.Stats.Attack.ShouldBe(8);
        persistedHero.Stats.MinimumDamage.ShouldBe(3);
        persistedHero.Stats.MaximumDamage.ShouldBe(7);
        persistedHero.Stats.Initiative.ShouldBe(10.5);
        persistedHero.Morale.Value.ShouldBe(4);
        persistedHero.Luck.Value.ShouldBe(2);
        persistedHero.FactionId.ShouldBe(faction.Id);
    }

    [Fact(DisplayName = "GetByIdAsync should return an existing hero when hero exists")]
    public async Task GetByIdAsync_Should_ReturnHero_When_HeroExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var faction = IntegrationTestData.CreateFaction();
        var hero = IntegrationTestData.CreateHero(faction);
        await AddFactionAndHeroesAsync(
            faction,
            cancellationToken,
            hero);

        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IHeroesRepository>();
        var persistedHero = await repository.GetByIdAsync(
            hero.Id,
            cancellationToken);

        persistedHero.ShouldNotBeNull();
        persistedHero.Name.Value.ShouldBe("Orrin");
        persistedHero.FactionId.ShouldBe(faction.Id);
    }

    [Fact(DisplayName = "UpdateAsync should persist hero changes when hero exists")]
    public async Task UpdateAsync_Should_PersistChanges_When_HeroExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var faction = IntegrationTestData.CreateFaction();
        var hero = IntegrationTestData.CreateHero(faction);
        await AddFactionAndHeroesAsync(
            faction,
            cancellationToken,
            hero);

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider
                .GetRequiredService<IHeroesRepository>();
            var dbContext = scope.ServiceProvider
                .GetRequiredService<CompendiumWriteDbContext>();
            var heroToUpdate = await repository.GetByIdAsync(
                hero.Id,
                cancellationToken);

            heroToUpdate.ShouldNotBeNull();
            heroToUpdate.Update(new HeroAttributes
            {
                Name = "Elara",
                Description = "An agile vanguard commander.",
                Attack = 10,
                Defense = 7,
                MinimumDamage = 4,
                MaximumDamage = 9,
                Initiative = 12.25,
                Morale = 5,
                Luck = 3,
                FactionId = faction.Id.Value
            })
                .IsSuccess.ShouldBeTrue();

            await repository.UpdateAsync(heroToUpdate, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        await using var verificationScope = Fixture.CreateScope();
        var verificationDbContext = verificationScope.ServiceProvider
            .GetRequiredService<CompendiumWriteDbContext>();
        var persistedHero = await verificationDbContext.Set<Hero>()
            .AsNoTracking()
            .SingleAsync(item => item.Id == hero.Id, cancellationToken);

        persistedHero.Name.Value.ShouldBe("Elara");
        persistedHero.Stats.Attack.ShouldBe(10);
        persistedHero.Stats.MaximumDamage.ShouldBe(9);
        persistedHero.Morale.Value.ShouldBe(5);
    }

    [Fact(DisplayName = "DeleteAsync should soft delete an existing hero when hero exists")]
    public async Task DeleteAsync_Should_SoftDeleteHero_When_HeroExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var faction = IntegrationTestData.CreateFaction();
        var hero = IntegrationTestData.CreateHero(faction);
        await AddFactionAndHeroesAsync(
            faction,
            cancellationToken,
            hero);

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider
                .GetRequiredService<IHeroesRepository>();
            var dbContext = scope.ServiceProvider
                .GetRequiredService<CompendiumWriteDbContext>();
            var persistedHero = await repository.GetByIdAsync(
                hero.Id,
                cancellationToken);

            persistedHero.ShouldNotBeNull();
            await repository.DeleteAsync(persistedHero, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        await using var verificationScope = Fixture.CreateScope();
        var verificationRepository = verificationScope.ServiceProvider
            .GetRequiredService<IHeroesRepository>();
        (await verificationRepository.GetByIdAsync(
                hero.Id,
                cancellationToken))
            .ShouldBeNull();
    }

    private async Task AddFactionAndHeroesAsync(
        Faction faction,
        CancellationToken cancellationToken,
        params Hero[] heroes)
    {
        await using var scope = Fixture.CreateScope();
        var factionsRepository = scope.ServiceProvider
            .GetRequiredService<IFactionsRepository>();
        var heroesRepository = scope.ServiceProvider
            .GetRequiredService<IHeroesRepository>();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<CompendiumWriteDbContext>();

        await factionsRepository.AddAsync(faction, cancellationToken);

        foreach (var hero in heroes)
        {
            await heroesRepository.AddAsync(hero, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
