namespace PANiXiDA.TacticalHeroes.Compendium.FunctionalTests.Presentation;

public sealed class PermissionAuthorizationTests(FunctionalTestFixture fixture) : FunctionalTestBase(fixture)
{
    [Theory(DisplayName = "Get should require permission when authenticated")]
    [InlineData("/api/v1/factions?pageNumber=1&pageSize=20", "compendium.factions.read")]
    [InlineData("/api/v1/heroes?pageNumber=1&pageSize=20", "compendium.heroes.read")]
    [InlineData("/api/v1/units?pageNumber=1&pageSize=20", "compendium.units.read")]
    [InlineData("/api/v1/factions/select-options?limit=20", "compendium.factions.read")]
    public async Task Get_Should_RequirePermission_When_Authenticated(string path, string permission)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var denied = new HttpRequestMessage(HttpMethod.Get, path);
        denied.Headers.Add("X-Test-Permissions", "unrelated");
        using var deniedResponse = await Fixture.Client.SendAsync(denied, cancellationToken);
        deniedResponse.StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        using var allowed = new HttpRequestMessage(HttpMethod.Get, path);
        allowed.Headers.Add("X-Test-Permissions", permission);
        using var allowedResponse = await Fixture.Client.SendAsync(allowed, cancellationToken);
        allowedResponse.StatusCode.ShouldBe(
            HttpStatusCode.OK,
            await allowedResponse.Content.ReadAsStringAsync(cancellationToken));
    }

    [Theory(DisplayName = "Get should return forbidden when only form permission is granted")]
    [InlineData("compendium.heroes.create")]
    [InlineData("compendium.heroes.update")]
    [InlineData("compendium.units.create")]
    [InlineData("compendium.units.update")]
    public async Task Get_Should_ReturnForbidden_When_OnlyFormPermissionIsGranted(string permission)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/factions/select-options?limit=20");
        request.Headers.Add("X-Test-Permissions", permission);

        using var response = await Fixture.Client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Theory(DisplayName = "Delete should return forbidden when only read permission is granted")]
    [InlineData("factions")]
    [InlineData("heroes")]
    [InlineData("units")]
    public async Task Delete_Should_ReturnForbidden_When_OnlyReadPermissionIsGranted(string resource)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/{resource}/{Guid.CreateVersion7()}");
        request.Headers.Add("X-Test-Permissions", $"compendium.{resource}.read");

        using var response = await Fixture.Client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
}
