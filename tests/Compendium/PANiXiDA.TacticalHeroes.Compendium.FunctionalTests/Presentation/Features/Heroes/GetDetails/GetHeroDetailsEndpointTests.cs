namespace PANiXiDA.TacticalHeroes.Compendium.FunctionalTests.Presentation.Features.Heroes.GetDetails;

public sealed class GetHeroDetailsEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "GET hero should return hero details when hero exists")]
    public async Task GetHero_Should_ReturnDetails_When_HeroExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new HeroesApiTestClient(Fixture);
        var faction = await client.CreateFactionAsync(cancellationToken);
        var createdHero = await client.CreateAsync(
            faction.Id,
            cancellationToken);

        var hero = await client.GetDetailsAsync(
            createdHero.Id,
            cancellationToken);

        hero.Id.ShouldBe(createdHero.Id);
        hero.Name.ShouldBe("Orrin");
        hero.FactionId.ShouldBe(faction.Id);
    }

    [Fact(DisplayName = "GET hero should return not found when hero does not exist")]
    public async Task GetHero_Should_ReturnNotFound_When_HeroDoesNotExist()
    {
        var client = new HeroesApiTestClient(Fixture);

        using var response = await client.GetDetailsResponseAsync(
            Guid.CreateVersion7(),
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
