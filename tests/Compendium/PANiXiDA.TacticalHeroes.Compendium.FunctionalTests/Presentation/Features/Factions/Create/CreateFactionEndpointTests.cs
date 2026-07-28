using PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Factions.Create;

namespace PANiXiDA.TacticalHeroes.Compendium.FunctionalTests.Presentation.Features.Factions.Create;

public sealed class CreateFactionEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "POST factions should create a normalized faction when request is valid")]
    public async Task PostFactions_Should_CreateFaction_When_RequestIsValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new FactionsApiTestClient(Fixture);

        var createdFaction = await client.CreateAsync(
            cancellationToken,
            new CreateFactionRequest(
                "  Northern Alliance  ",
                "  Defenders of the north.  "));
        var faction = await client.GetDetailsAsync(
            createdFaction.Id,
            cancellationToken);

        faction.Name.ShouldBe("Northern Alliance");
        faction.Description.ShouldBe("Defenders of the north.");
    }
}
