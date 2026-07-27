using PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Factions.Create;

namespace PANiXiDA.TacticalHeroes.Compendium.FunctionalTests.Presentation.Features.Factions.GetList;

public sealed class GetFactionsEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "GET factions should return a sorted page")]
    public async Task GetFactions_Should_ReturnSortedPage_When_FactionsExist()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new FactionsApiTestClient(Fixture);
        await client.CreateAsync(
            cancellationToken,
            new CreateFactionRequest(
                "Southern Alliance",
                "Defenders of the south."));
        await client.CreateAsync(
            cancellationToken,
            new CreateFactionRequest(
                "Northern Alliance",
                "Defenders of the north."));

        var response = await client.GetListAsync(cancellationToken);

        response.TotalCount.ShouldBe(2);
        response.Items.Select(faction => faction.Name)
            .ShouldBe(["Northern Alliance", "Southern Alliance"]);
    }
}
