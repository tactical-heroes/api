namespace PANiXiDA.TacticalHeroes.Compendium.FunctionalTests.Presentation.Features.Units.Delete;

public sealed class DeleteUnitEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "DELETE unit should remove unit from reads when unit exists")]
    public async Task DeleteUnit_Should_HideUnit_When_UnitExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new UnitsApiTestClient(Fixture);
        var faction = await client.CreateFactionAsync(cancellationToken);
        var createdUnit = await client.CreateAsync(
            faction.Id,
            cancellationToken);

        await client.DeleteAsync(createdUnit.Id, cancellationToken);
        using var response = await client.GetDetailsResponseAsync(
            createdUnit.Id,
            cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
