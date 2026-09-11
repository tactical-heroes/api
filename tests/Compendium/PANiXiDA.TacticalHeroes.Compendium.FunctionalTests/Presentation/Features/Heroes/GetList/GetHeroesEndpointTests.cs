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
        const string factionName = "Northern Alliance";
        var faction = await client.CreateFactionAsync(
            cancellationToken,
            new CreateFactionRequest(
                factionName,
                "Defenders of the north."));
        await client.CreateAsync(
            faction.Id,
            cancellationToken,
            HeroesApiTestClient.CreateRequest(
                factionId: faction.Id,
                name: "Orrin"));
        await client.CreateAsync(
            faction.Id,
            cancellationToken,
            HeroesApiTestClient.CreateRequest(
                factionId: faction.Id,
                name: "Elara"));

        var response = await client.GetListAsync(cancellationToken);

        response.TotalCount.ShouldBe(2);
        response.Items.Select(hero => hero.Name)
            .ShouldBe(["Elara", "Orrin"]);
        response.Items.ShouldAllBe(hero => hero.FactionId == faction.Id);
        response.Items.ShouldAllBe(hero => hero.FactionName == factionName);
    }
}
