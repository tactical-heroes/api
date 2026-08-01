namespace PANiXiDA.TacticalHeroes.Compendium.FunctionalTests.Presentation.Features.Units.GetDetails;

public sealed class GetUnitDetailsEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "GET unit should return unit details when unit exists")]
    public async Task GetUnit_Should_ReturnDetails_When_UnitExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new UnitsApiTestClient(Fixture);
        var faction = await client.CreateFactionAsync(cancellationToken);
        var createdUnit = await client.CreateAsync(
            faction.Id,
            cancellationToken);

        var unit = await client.GetDetailsAsync(
            createdUnit.Id,
            cancellationToken);

        unit.Id.ShouldBe(createdUnit.Id);
        unit.Name.ShouldBe("Archer");
        unit.FactionId.ShouldBe(faction.Id);
    }

    [Fact(DisplayName = "GET unit should return not found when unit does not exist")]
    public async Task GetUnit_Should_ReturnNotFound_When_UnitDoesNotExist()
    {
        var client = new UnitsApiTestClient(Fixture);

        using var response = await client.GetDetailsResponseAsync(
            Guid.CreateVersion7(),
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
