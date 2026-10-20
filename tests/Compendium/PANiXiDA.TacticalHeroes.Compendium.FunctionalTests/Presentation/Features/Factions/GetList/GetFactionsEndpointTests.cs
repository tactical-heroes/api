using PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Factions.Create;

namespace PANiXiDA.TacticalHeroes.Compendium.FunctionalTests.Presentation.Features.Factions.GetList;

public sealed class GetFactionsEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "GET factions should return a sorted page when factions exist")]
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

    [Theory(DisplayName = "GET factions should require read permission when authenticated")]
    [InlineData("unrelated", HttpStatusCode.Forbidden)]
    [InlineData("compendium.factions.read", HttpStatusCode.OK)]
    public async Task GetFactions_Should_RequireReadPermission_When_Authenticated(
        string permission, HttpStatusCode expectedStatus)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/factions?pageNumber=1&pageSize=20");
        request.Headers.Add("X-Test-Permissions", permission);

        using var response = await Fixture.Client.SendAsync(request, cancellationToken);

        response.StatusCode.ShouldBe(expectedStatus, await response.Content.ReadAsStringAsync(cancellationToken));
    }
}
