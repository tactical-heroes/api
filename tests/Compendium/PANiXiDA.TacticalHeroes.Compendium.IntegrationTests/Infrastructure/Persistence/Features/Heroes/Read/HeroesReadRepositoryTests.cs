using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Compendium.IntegrationTests.Heroes;

namespace PANiXiDA.TacticalHeroes.Compendium.IntegrationTests.Infrastructure.Persistence.Features.Heroes.Read;

public sealed class HeroesReadRepositoryTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "GetDetailsByIdAsync should return hero details when hero exists")]
    public async Task GetDetailsByIdAsync_Should_ReturnDetails_When_HeroExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var faction = IntegrationTestData.CreateFaction();
        var hero = IntegrationTestData.CreateHero(faction);
        await AddFactionAndHeroesAsync(
            faction,
            cancellationToken,
            hero);

        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider
            .GetRequiredService<IHeroesReadRepository>();
        var details = await repository.GetDetailsByIdAsync(
            hero.Id.Value,
            cancellationToken);

        details.ShouldNotBeNull();
        details.Name.ShouldBe("Orrin");
        details.Attack.ShouldBe(8);
        details.Defense.ShouldBe(6);
        details.MinimumDamage.ShouldBe(3);
        details.MaximumDamage.ShouldBe(7);
        details.Initiative.ShouldBe(10.5);
        details.Morale.ShouldBe(4);
        details.Luck.ShouldBe(2);
        details.FactionId.ShouldBe(faction.Id.Value);
    }

    [Fact(DisplayName = "GetPageAsync should return heroes sorted by name when heroes exist")]
    public async Task GetPageAsync_Should_ReturnSortedPage_When_HeroesExist()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var faction = IntegrationTestData.CreateFaction();
        var elara = IntegrationTestData.CreateHero(faction, "Elara");
        var orrin = IntegrationTestData.CreateHero(faction, "Orrin");
        await AddFactionAndHeroesAsync(
            faction,
            cancellationToken,
            orrin,
            elara);

        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider
            .GetRequiredService<IHeroesReadRepository>();
        var page = await repository.GetPageAsync(
            new PaginationParameters(1, 20),
            cancellationToken);

        page.TotalCount.ShouldBe(2);
        page.Items.Select(item => item.Name)
            .ShouldBe(["Elara", "Orrin"]);
        page.Items.ShouldAllBe(item => item.FactionId == faction.Id.Value);
        page.Items.ShouldAllBe(item => item.FactionName == faction.Name.Value);
    }

    [Fact(DisplayName = "ExistsByIdAsync should return true when hero exists")]
    public async Task ExistsByIdAsync_Should_ReturnTrue_When_HeroExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var faction = IntegrationTestData.CreateFaction();
        var hero = IntegrationTestData.CreateHero(faction);
        await AddFactionAndHeroesAsync(
            faction,
            cancellationToken,
            hero);

        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider
            .GetRequiredService<IHeroesReadRepository>();

        (await repository.ExistsByIdAsync(hero.Id.Value, cancellationToken))
            .ShouldBeTrue();
    }

    [Fact(DisplayName = "AnyAsync should reflect whether heroes exist when called")]
    public async Task AnyAsync_Should_ReflectWhetherHeroesExist_When_Called()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        await using (var emptyScope = Fixture.CreateScope())
        {
            var emptyRepository = emptyScope.ServiceProvider
                .GetRequiredService<IHeroesReadRepository>();
            (await emptyRepository.AnyAsync(cancellationToken)).ShouldBeFalse();
        }

        var faction = IntegrationTestData.CreateFaction();
        await AddFactionAndHeroesAsync(
            faction,
            cancellationToken,
            IntegrationTestData.CreateHero(faction));

        await using var populatedScope = Fixture.CreateScope();
        var populatedRepository = populatedScope.ServiceProvider
            .GetRequiredService<IHeroesReadRepository>();
        (await populatedRepository.AnyAsync(cancellationToken)).ShouldBeTrue();
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
