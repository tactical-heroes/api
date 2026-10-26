namespace PANiXiDA.TacticalHeroes.Compendium.FunctionalTests.Presentation.Features.Factions.Delete;

public sealed class DeleteFactionEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "DELETE faction should remove faction from reads when faction exists")]
    public async Task DeleteFaction_Should_HideFaction_When_FactionExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new FactionsApiTestClient(Fixture);
        var createdFaction = await client.CreateAsync(request: null, cancellationToken: cancellationToken);

        await client.DeleteAsync(createdFaction.Id, cancellationToken);
        using var response = await client.GetDetailsResponseAsync(
            createdFaction.Id,
            cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "DELETE faction should return forbidden when only read permission is granted")]
    public async Task DeleteFaction_Should_ReturnForbidden_When_OnlyReadPermissionIsGranted()
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/factions/{Guid.CreateVersion7()}");
        request.Headers.Add("X-Test-Permissions", "compendium.factions.read");

        using var response = await Fixture.Client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
}
