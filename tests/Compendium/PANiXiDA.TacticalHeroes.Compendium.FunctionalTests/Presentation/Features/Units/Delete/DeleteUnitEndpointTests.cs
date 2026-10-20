using System.Text.Json;

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

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

    [Fact(DisplayName = "DELETE unit should return conflict when another transaction deletes the loaded unit")]
    public async Task DeleteUnit_Should_ReturnConflict_When_AnotherTransactionDeletesTheLoadedUnit()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var api = new UnitsApiTestClient(Fixture);
        var faction = await api.CreateFactionAsync(cancellationToken);
        var unit = await api.CreateAsync(faction.Id, cancellationToken);
        var interceptor = new DeleteUnitBeforeSaveInterceptor(unit.Id, Fixture.Services);
        await using var testFactory = new FunctionalTestWebApplicationFactory();
        await using var factory = testFactory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => services.AddSingleton<IInterceptor>(interceptor)));
        using var client = factory.CreateClient();

        using var response = await client.DeleteAsync($"/api/v1/units/{unit.Id}", cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        interceptor.UnitDeleted.ShouldBeTrue();
        interceptor.RequestHadTransaction.ShouldBeTrue();
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict, responseBody);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
        var problem = JsonSerializer.Deserialize<JsonElement>(responseBody);
        problem.GetProperty("status").GetInt32().ShouldBe(409);
        problem.GetProperty("title").GetString().ShouldBe("Conflict");
        problem.GetProperty("detail").GetString()
            .ShouldBe("The entity was modified or deleted. Reload it and try again.");
        using var deletedUnitResponse = await client.GetAsync($"/api/v1/units/{unit.Id}", cancellationToken);
        deletedUnitResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        using var nextResponse = await client.PostAsJsonAsync(
            "/api/v1/units",
            UnitsApiTestClient.CreateRequest(faction.Id, "Next unit"),
            cancellationToken);
        nextResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact(DisplayName = "DELETE unit should return forbidden when only read permission is granted")]
    public async Task DeleteUnit_Should_ReturnForbidden_When_OnlyReadPermissionIsGranted()
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/units/{Guid.CreateVersion7()}");
        request.Headers.Add("X-Test-Permissions", "compendium.units.read");

        using var response = await Fixture.Client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
}
