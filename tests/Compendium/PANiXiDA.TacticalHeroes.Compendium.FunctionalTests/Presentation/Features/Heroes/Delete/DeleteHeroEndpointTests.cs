namespace PANiXiDA.TacticalHeroes.Compendium.FunctionalTests.Presentation.Features.Heroes.Delete;

public sealed class DeleteHeroEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "DELETE hero should remove hero from reads when hero exists")]
    public async Task DeleteHero_Should_HideHero_When_HeroExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new HeroesApiTestClient(Fixture);
        var faction = await client.CreateFactionAsync(cancellationToken);
        var createdHero = await client.CreateAsync(
            faction.Id,
            cancellationToken);

        await client.DeleteAsync(createdHero.Id, cancellationToken);
        using var response = await client.GetDetailsResponseAsync(
            createdHero.Id,
            cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
