using System.Text.Json;

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Units.Update;

namespace PANiXiDA.TacticalHeroes.Compendium.FunctionalTests.Presentation.Features.Units.Update;

public sealed class UpdateUnitEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Theory(DisplayName = "PUT unit should update details and faction when ranged attack fields are omitted or null")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PutUnit_Should_UpdateDetailsAndFaction_When_RangedAttackFieldsAreOmittedOrNull(bool omitFields)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new UnitsApiTestClient(Fixture);
        var originalFaction = await client.CreateFactionAsync(cancellationToken);
        var targetFaction = await client.CreateFactionAsync(cancellationToken);
        var createdUnit = await client.CreateAsync(
            originalFaction.Id,
            cancellationToken);

        var request = new UpdateUnitRequest(
            Name: "Swordsman",
            Description: "A disciplined melee unit.",
            Attack: 9,
            Defense: 10,
            Health: 20,
            MinimumDamage: 4,
            MaximumDamage: 6,
            Initiative: 9.5,
            Speed: 5,
            Morale: 3,
            Luck: 2,
            FactionId: targetFaction.Id,
            Shots: null,
            RangedAttackRange: null);
        var payload = JsonSerializer.SerializeToNode(request, TestJsonSerializerOptions.Web)!.AsObject();
        if (omitFields)
        {
            payload.Remove("shots");
            payload.Remove("rangedAttackRange");
        }

        using var response = await Fixture.Client.PutAsJsonAsync(
            $"/api/v1/units/{createdUnit.Id}", payload, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent, responseBody);
        var unit = await client.GetDetailsAsync(
            createdUnit.Id,
            cancellationToken);

        unit.Name.ShouldBe("Swordsman");
        unit.Attack.ShouldBe(9);
        unit.Shots.ShouldBeNull();
        unit.RangedAttackRange.ShouldBeNull();
        unit.Morale.ShouldBe(3);
        unit.Luck.ShouldBe(2);
        unit.FactionId.ShouldBe(targetFaction.Id);
    }

    [Fact(DisplayName = "PUT unit should return conflict when another transaction deletes the loaded unit")]
    public async Task PutUnit_Should_ReturnConflict_When_AnotherTransactionDeletesTheLoadedUnit()
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
        var request = new UpdateUnitRequest(
            Name: "Swordsman",
            Description: "A disciplined melee unit.",
            Attack: 9,
            Defense: 10,
            Health: 20,
            MinimumDamage: 4,
            MaximumDamage: 6,
            Initiative: 9.5,
            Speed: 5,
            Morale: 3,
            Luck: 2,
            FactionId: faction.Id,
            Shots: null,
            RangedAttackRange: null);

        using var response = await client.PutAsJsonAsync(
            $"/api/v1/units/{unit.Id}", request, cancellationToken);

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
}
