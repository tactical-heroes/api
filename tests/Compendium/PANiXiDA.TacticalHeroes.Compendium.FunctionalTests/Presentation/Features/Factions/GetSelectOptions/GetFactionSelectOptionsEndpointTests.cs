using System.Text.Json;

using PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Factions.Create;

namespace PANiXiDA.TacticalHeroes.Compendium.FunctionalTests.Presentation.Features.Factions.GetSelectOptions;

public sealed class GetFactionSelectOptionsEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Theory(DisplayName = "GetFactionSelectOptions should return matching options when search and limit are supplied")]
    [InlineData(" ALLiance ")]
    [InlineData("nor")]
    public async Task GetFactionSelectOptions_Should_ReturnMatchingOptions_When_SearchAndLimitAreSupplied(string search)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new FactionsApiTestClient(Fixture);
        await client.CreateAsync(
            new CreateFactionRequest("Southern Alliance", "South."),
            cancellationToken);
        var northern = await client.CreateAsync(
            new CreateFactionRequest("Northern Alliance", "North."),
            cancellationToken);
        await client.CreateAsync(new CreateFactionRequest("Empire", "An alliance."), cancellationToken);

        using var response = await Fixture.Client.GetAsync(
            $"/api/v1/factions/select-options?search={Uri.EscapeDataString(search)}&limit=1",
            cancellationToken);
        var options = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        options.GetArrayLength().ShouldBe(1);
        options[0].GetProperty("id").GetGuid().ShouldBe(northern.Id);
        options[0].GetProperty("name").GetString().ShouldBe("Northern Alliance");
        options[0].EnumerateObject().Select(property => property.Name).ShouldBe(["id", "name"]);
    }

    [Fact(DisplayName = "GetFactionSelectOptions should return twenty options when limit is omitted")]
    public async Task GetFactionSelectOptions_Should_ReturnTwentyOptions_When_LimitIsOmitted()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new FactionsApiTestClient(Fixture);
        for (int index = 0; index < 21; index++)
        {
            await client.CreateAsync(
                new CreateFactionRequest($"Faction {index:D2}", "A faction."),
                cancellationToken);
        }

        using var response = await Fixture.Client.GetAsync("/api/v1/factions/select-options", cancellationToken);
        var options = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        options.GetArrayLength().ShouldBe(20);
        options[0].GetProperty("name").GetString().ShouldBe("Faction 00");
        options[19].GetProperty("name").GetString().ShouldBe("Faction 19");
    }

    [Theory(DisplayName = "GetFactionSelectOptions should return validation problem when limit is invalid")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(201)]
    public async Task GetFactionSelectOptions_Should_ReturnValidationProblem_When_LimitIsInvalid(int limit)
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        using var response = await Fixture.Client
            .GetAsync($"/api/v1/factions/select-options?limit={limit}", cancellationToken);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        problem.GetProperty("errors").EnumerateObject().ShouldNotBeEmpty();
    }

    [Theory(
        DisplayName = "GetFactionSelectOptions should return validation problem when trimmed search is shorter than three characters")]
    [InlineData("")]
    [InlineData("n")]
    [InlineData("no")]
    [InlineData("   ")]
    [InlineData(" no ")]
    public async Task
        GetFactionSelectOptions_Should_ReturnValidationProblem_When_TrimmedSearchIsShorterThanThreeCharacters(
            string search)
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        using var response = await Fixture.Client
            .GetAsync($"/api/v1/factions/select-options?search={Uri.EscapeDataString(search)}", cancellationToken);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        problem.GetProperty("errors").EnumerateObject().ShouldNotBeEmpty();
    }

    [Fact(DisplayName = "GetFactionSelectOptions should return validation problem when search is too long")]
    public async Task GetFactionSelectOptions_Should_ReturnValidationProblem_When_SearchIsTooLong()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var search = new string('a', 129);

        using var response = await Fixture.Client
            .GetAsync($"/api/v1/factions/select-options?search={search}", cancellationToken);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        problem.GetProperty("errors").EnumerateObject().ShouldNotBeEmpty();
    }

    [Fact(DisplayName = "GetFactionSelectOptions should accept maximum limit when limit is two hundred")]
    public async Task GetFactionSelectOptions_Should_AcceptMaximumLimit_When_LimitIsTwoHundred()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new FactionsApiTestClient(Fixture);
        var faction = await client.CreateAsync(
            new CreateFactionRequest("Northern Alliance", "North."),
            cancellationToken);

        using var response = await Fixture.Client
            .GetAsync("/api/v1/factions/select-options?limit=200", cancellationToken);
        var options = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        options.GetArrayLength().ShouldBe(1);
        options[0].GetProperty("id").GetGuid().ShouldBe(faction.Id);
    }

    [Theory(DisplayName = "GET faction select options should require read permission when authenticated")]
    [InlineData("unrelated", HttpStatusCode.Forbidden)]
    [InlineData("compendium.heroes.create", HttpStatusCode.Forbidden)]
    [InlineData("compendium.heroes.update", HttpStatusCode.Forbidden)]
    [InlineData("compendium.units.create", HttpStatusCode.Forbidden)]
    [InlineData("compendium.units.update", HttpStatusCode.Forbidden)]
    [InlineData("compendium.factions.read", HttpStatusCode.OK)]
    public async Task GetFactionSelectOptions_Should_RequireReadPermission_When_Authenticated(
        string permission, HttpStatusCode expectedStatus)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/factions/select-options?limit=20");
        request.Headers.Add("X-Test-Permissions", permission);

        using var response = await Fixture.Client.SendAsync(request, cancellationToken);

        response.StatusCode.ShouldBe(expectedStatus, await response.Content.ReadAsStringAsync(cancellationToken));
    }
}
