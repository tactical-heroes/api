using PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Factions.Create;

namespace PANiXiDA.TacticalHeroes.Compendium.FunctionalTests.Presentation.Features.Factions.GetDetails;

public sealed class GetFactionDetailsEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "GET faction should return faction details when faction exists")]
    public async Task GetFaction_Should_ReturnDetails_When_FactionExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new FactionsApiTestClient(Fixture);
        var createdFaction = await client.CreateAsync(
            cancellationToken,
            new CreateFactionRequest(
                "Northern Alliance",
                "Defenders of the north."));

        var faction = await client.GetDetailsAsync(
            createdFaction.Id,
            cancellationToken);

        faction.Id.ShouldBe(createdFaction.Id);
        faction.Name.ShouldBe("Northern Alliance");
        faction.Description.ShouldBe("Defenders of the north.");
    }

    [Fact(DisplayName = "GET faction should return not found for a missing faction when faction does not exist")]
    public async Task GetFaction_Should_ReturnNotFound_When_FactionDoesNotExist()
    {
        var client = new FactionsApiTestClient(Fixture);

        using var response = await client.GetDetailsResponseAsync(
            Guid.CreateVersion7(),
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
