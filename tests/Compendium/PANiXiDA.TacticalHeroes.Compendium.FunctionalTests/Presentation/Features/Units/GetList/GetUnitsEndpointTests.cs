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
        const string factionName = "Northern Alliance";
        var faction = await client.CreateFactionAsync(
            cancellationToken,
            new CreateFactionRequest(
                factionName,
                "Defenders of the north."));
        await client.CreateAsync(
            faction.Id,
            cancellationToken,
            UnitsApiTestClient.CreateRequest(
                factionId: faction.Id,
                name: "Marksman"));
        await client.CreateAsync(
            faction.Id,
            cancellationToken,
            UnitsApiTestClient.CreateRequest(
                factionId: faction.Id,
                name: "Archer"));

        var response = await client.GetListAsync(cancellationToken);

        response.TotalCount.ShouldBe(2);
        response.Items.Select(unit => unit.Name)
            .ShouldBe(["Archer", "Marksman"]);
        response.Items.ShouldAllBe(unit => unit.FactionId == faction.Id);
        response.Items.ShouldAllBe(unit => unit.FactionName == factionName);
    }
}
