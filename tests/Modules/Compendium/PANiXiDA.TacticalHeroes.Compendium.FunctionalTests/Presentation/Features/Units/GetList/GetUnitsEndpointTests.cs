using PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Factions.Create;

namespace PANiXiDA.TacticalHeroes.Compendium.FunctionalTests.Presentation.Features.Units.GetList;

public sealed class GetUnitsEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "GET units should return a sorted page when units exist")]
    public async Task GetUnits_Should_ReturnSortedPage_When_UnitsExist()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new UnitsApiTestClient(Fixture);
        const string FactionName = "Northern Alliance";
        var faction = await client.CreateFactionAsync(
            new CreateFactionRequest(
                FactionName,
                "Defenders of the north."),
            cancellationToken);
        await client.CreateAsync(
            faction.Id,
            UnitsApiTestClient.CreateRequest(
                factionId: faction.Id,
                name: "Marksman"),
            cancellationToken);
        await client.CreateAsync(
            faction.Id,
            UnitsApiTestClient.CreateRequest(
                factionId: faction.Id,
                name: "Archer"),
            cancellationToken);

        var response = await client.GetListAsync(cancellationToken);

        response.TotalCount.ShouldBe(2);
        response.Items.Select(unit => unit.Name)
            .ShouldBe(["Archer", "Marksman"]);
        response.Items.ShouldAllBe(unit => unit.FactionId == faction.Id);
        response.Items.ShouldAllBe(unit => unit.FactionName == FactionName);
    }

    [Theory(DisplayName = "GET units should require read permission when authenticated")]
    [InlineData("unrelated", HttpStatusCode.Forbidden)]
    [InlineData("compendium.units.read", HttpStatusCode.OK)]
    public async Task GetUnits_Should_RequireReadPermission_When_Authenticated(
        string permission, HttpStatusCode expectedStatus)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/units?pageNumber=1&pageSize=20");
        request.Headers.Add("X-Test-Permissions", permission);

        using var response = await Fixture.Client.SendAsync(request, cancellationToken);

        response.StatusCode.ShouldBe(expectedStatus, await response.Content.ReadAsStringAsync(cancellationToken));
    }
}
