using PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Factions.Create;

namespace PANiXiDA.TacticalHeroes.Compendium.FunctionalTests.Presentation.Features.Heroes.GetList;

public sealed class GetHeroesEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "GET heroes should return a sorted page when heroes exist")]
    public async Task GetHeroes_Should_ReturnSortedPage_When_HeroesExist()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new HeroesApiTestClient(Fixture);
        const string FactionName = "Northern Alliance";
        var faction = await client.CreateFactionAsync(
            new CreateFactionRequest(
                FactionName,
                "Defenders of the north."),
            cancellationToken);
        await client.CreateAsync(
            faction.Id,
            HeroesApiTestClient.CreateRequest(
                factionId: faction.Id,
                name: "Orrin"),
            cancellationToken);
        await client.CreateAsync(
            faction.Id,
            HeroesApiTestClient.CreateRequest(
                factionId: faction.Id,
                name: "Elara"),
            cancellationToken);

        var response = await client.GetListAsync(cancellationToken);

        response.TotalCount.ShouldBe(2);
        response.Items.Select(hero => hero.Name)
            .ShouldBe(["Elara", "Orrin"]);
        response.Items.ShouldAllBe(hero => hero.FactionId == faction.Id);
        response.Items.ShouldAllBe(hero => hero.FactionName == FactionName);
    }

    [Theory(DisplayName = "GET heroes should require read permission when authenticated")]
    [InlineData("unrelated", HttpStatusCode.Forbidden)]
    [InlineData("compendium.heroes.read", HttpStatusCode.OK)]
    public async Task GetHeroes_Should_RequireReadPermission_When_Authenticated(
        string permission, HttpStatusCode expectedStatus)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/heroes?pageNumber=1&pageSize=20");
        request.Headers.Add("X-Test-Permissions", permission);

        using var response = await Fixture.Client.SendAsync(request, cancellationToken);

        response.StatusCode.ShouldBe(expectedStatus, await response.Content.ReadAsStringAsync(cancellationToken));
    }
}
