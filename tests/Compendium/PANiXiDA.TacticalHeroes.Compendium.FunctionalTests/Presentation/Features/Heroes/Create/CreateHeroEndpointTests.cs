using PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Heroes.Create;

namespace PANiXiDA.TacticalHeroes.Compendium.FunctionalTests.Presentation.Features.Heroes.Create;

public sealed class CreateHeroEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "POST heroes should create a normalized hero when request is valid")]
    public async Task PostHeroes_Should_CreateHero_When_RequestIsValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new HeroesApiTestClient(Fixture);
        var faction = await client.CreateFactionAsync(cancellationToken);

        var createdHero = await client.CreateAsync(
            faction.Id,
            cancellationToken,
            new CreateHeroRequest(
                Name: "  Orrin  ",
                Description: "  A seasoned northern commander.  ",
                Attack: 8,
                Defense: 6,
                MinimumDamage: 3,
                MaximumDamage: 7,
                Initiative: 10.5,
                Morale: 4,
                Luck: 2,
                FactionId: faction.Id));
        var hero = await client.GetDetailsAsync(
            createdHero.Id,
            cancellationToken);

        hero.Name.ShouldBe("Orrin");
        hero.Description.ShouldBe("A seasoned northern commander.");
        hero.Attack.ShouldBe(8);
        hero.Defense.ShouldBe(6);
        hero.MinimumDamage.ShouldBe(3);
        hero.MaximumDamage.ShouldBe(7);
        hero.Initiative.ShouldBe(10.5);
        hero.Morale.ShouldBe(4);
        hero.Luck.ShouldBe(2);
        hero.FactionId.ShouldBe(faction.Id);
    }
}
