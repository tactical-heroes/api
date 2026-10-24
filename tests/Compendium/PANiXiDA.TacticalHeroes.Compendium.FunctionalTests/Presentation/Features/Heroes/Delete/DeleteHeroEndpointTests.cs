namespace PANiXiDA.TacticalHeroes.Compendium.FunctionalTests.Presentation.Features.Heroes.Delete;

public sealed class DeleteHeroEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "DELETE hero should remove hero from reads when hero exists")]
    public async Task DeleteHero_Should_HideHero_When_HeroExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new HeroesApiTestClient(Fixture);
        var faction = await client.CreateFactionAsync(request: null, cancellationToken: cancellationToken);
        var createdHero = await client.CreateAsync(
            factionId: faction.Id,
            request: null,
            cancellationToken: cancellationToken);

        await client.DeleteAsync(createdHero.Id, cancellationToken);
        using var response = await client.GetDetailsResponseAsync(
            createdHero.Id,
            cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "DELETE hero should return forbidden when only read permission is granted")]
    public async Task DeleteHero_Should_ReturnForbidden_When_OnlyReadPermissionIsGranted()
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/heroes/{Guid.CreateVersion7()}");
        request.Headers.Add("X-Test-Permissions", "compendium.heroes.read");

        using var response = await Fixture.Client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
}
