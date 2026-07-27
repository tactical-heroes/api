namespace PANiXiDA.TacticalHeroes.Compendium.FunctionalTests.Presentation.Features.Factions.Delete;

public sealed class DeleteFactionEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "DELETE faction should remove faction from reads")]
    public async Task DeleteFaction_Should_HideFaction_When_FactionExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new FactionsApiTestClient(Fixture);
        var createdFaction = await client.CreateAsync(cancellationToken);

        await client.DeleteAsync(createdFaction.Id, cancellationToken);
        using var response = await client.GetDetailsResponseAsync(
            createdFaction.Id,
            cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
