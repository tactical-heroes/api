using PANiXiDA.TacticalHeroes.Compendium.FunctionalTests.Presentation.Features.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Factions.Create;
using PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Units.Create;
using PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Units.GetDetails;
using PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Units.GetList;
using PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Units.Update;

namespace PANiXiDA.TacticalHeroes.Compendium.FunctionalTests.Presentation.Features.Units;

internal sealed class UnitsApiTestClient(FunctionalTestFixture fixture)
{
    private const string UnitsPath = "/api/v1/units";

    internal Task<CreateFactionResponse> CreateFactionAsync(
        CancellationToken cancellationToken)
    {
        return new FactionsApiTestClient(fixture).CreateAsync(cancellationToken);
    }

    internal async Task<CreateUnitResponse> CreateAsync(
        Guid factionId,
        CancellationToken cancellationToken,
        CreateUnitRequest? request = null)
    {
        request ??= CreateRequest(
            factionId: factionId,
            name: "Archer");
        using var response = await fixture.Client.PostAsJsonAsync(
            UnitsPath,
            request,
            TestJsonSerializerOptions.Web,
            cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Created, responseBody);
        response.Headers.Location.ShouldNotBeNull();

        return await response.Content.ReadFromJsonAsync<CreateUnitResponse>(
                TestJsonSerializerOptions.Web,
                cancellationToken)
            ?? throw new InvalidOperationException("Created unit was not returned.");
    }

    internal async Task<GetUnitDetailsResponse> GetDetailsAsync(
        Guid unitId,
        CancellationToken cancellationToken)
    {
        using var response = await fixture.Client.GetAsync(
            $"{UnitsPath}/{unitId}",
            cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, responseBody);

        return await response.Content.ReadFromJsonAsync<GetUnitDetailsResponse>(
                TestJsonSerializerOptions.Web,
                cancellationToken)
            ?? throw new InvalidOperationException("Unit details were not returned.");
    }

    internal async Task<PaginationResult<UnitListItemResponse>> GetListAsync(
        CancellationToken cancellationToken)
    {
        using var response = await fixture.Client.GetAsync(
            $"{UnitsPath}?pageNumber=1&pageSize=20",
            cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, responseBody);

        return await response.Content.ReadFromJsonAsync<
                PaginationResult<UnitListItemResponse>>(
                TestJsonSerializerOptions.Web,
                cancellationToken)
            ?? throw new InvalidOperationException("Unit page was not returned.");
    }

    internal async Task UpdateAsync(
        Guid unitId,
        UpdateUnitRequest request,
        CancellationToken cancellationToken)
    {
        using var response = await fixture.Client.PutAsJsonAsync(
            $"{UnitsPath}/{unitId}",
            request,
            TestJsonSerializerOptions.Web,
            cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent, responseBody);
    }

    internal async Task DeleteAsync(
        Guid unitId,
        CancellationToken cancellationToken)
    {
        using var response = await fixture.Client.DeleteAsync(
            $"{UnitsPath}/{unitId}",
            cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent, responseBody);
    }

    internal Task<HttpResponseMessage> GetDetailsResponseAsync(
        Guid unitId,
        CancellationToken cancellationToken)
    {
        return fixture.Client.GetAsync(
            $"{UnitsPath}/{unitId}",
            cancellationToken);
    }

    internal static CreateUnitRequest CreateRequest(
        Guid factionId,
        string name)
    {
        return new CreateUnitRequest(
            Name: name,
            Description: $"{name} description.",
            Attack: 8,
            Defense: 4,
            Health: 12,
            MinimumDamage: 3,
            MaximumDamage: 5,
            Initiative: 10.5,
            Speed: 6,
            Shots: 12,
            RangedAttackRange: 8,
            Morale: 2,
            Luck: 1,
            FactionId: factionId);
    }
}
