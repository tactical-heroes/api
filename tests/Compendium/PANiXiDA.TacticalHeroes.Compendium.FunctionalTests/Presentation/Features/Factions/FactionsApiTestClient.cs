using PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Factions.Create;
using PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Factions.GetDetails;
using PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Factions.GetList;
using PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Factions.Update;

namespace PANiXiDA.TacticalHeroes.Compendium.FunctionalTests.Presentation.Features.Factions;

internal sealed class FactionsApiTestClient(FunctionalTestFixture fixture)
{
    private const string FactionsPath = "/api/v1/factions";

    internal async Task<CreateFactionResponse> CreateAsync(
        CancellationToken cancellationToken,
        CreateFactionRequest? request = null)
    {
        request ??= new CreateFactionRequest(
            "Northern Alliance",
            "Defenders of the north.");
        using var response = await fixture.Client.PostAsJsonAsync(
            FactionsPath,
            request,
            TestJsonSerializerOptions.Web,
            cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Created, responseBody);
        response.Headers.Location.ShouldNotBeNull();

        return await response.Content.ReadFromJsonAsync<CreateFactionResponse>(
                TestJsonSerializerOptions.Web,
                cancellationToken)
            ?? throw new InvalidOperationException("Created faction was not returned.");
    }

    internal async Task<GetFactionDetailsResponse> GetDetailsAsync(
        Guid factionId,
        CancellationToken cancellationToken)
    {
        using var response = await fixture.Client.GetAsync(
            $"{FactionsPath}/{factionId}",
            cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, responseBody);

        return await response.Content.ReadFromJsonAsync<GetFactionDetailsResponse>(
                TestJsonSerializerOptions.Web,
                cancellationToken)
            ?? throw new InvalidOperationException("Faction details were not returned.");
    }

    internal async Task<PaginationResult<FactionListItemResponse>> GetListAsync(
        CancellationToken cancellationToken)
    {
        using var response = await fixture.Client.GetAsync(
            $"{FactionsPath}?pageNumber=1&pageSize=20",
            cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, responseBody);

        return await response.Content.ReadFromJsonAsync<
                PaginationResult<FactionListItemResponse>>(
                TestJsonSerializerOptions.Web,
                cancellationToken)
            ?? throw new InvalidOperationException("Faction page was not returned.");
    }

    internal async Task UpdateAsync(
        Guid factionId,
        UpdateFactionRequest request,
        CancellationToken cancellationToken)
    {
        using var response = await fixture.Client.PutAsJsonAsync(
            $"{FactionsPath}/{factionId}",
            request,
            TestJsonSerializerOptions.Web,
            cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent, responseBody);
    }

    internal async Task DeleteAsync(
        Guid factionId,
        CancellationToken cancellationToken)
    {
        using var response = await fixture.Client.DeleteAsync(
            $"{FactionsPath}/{factionId}",
            cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent, responseBody);
    }

    internal Task<HttpResponseMessage> GetDetailsResponseAsync(
        Guid factionId,
        CancellationToken cancellationToken)
    {
        return fixture.Client.GetAsync(
            $"{FactionsPath}/{factionId}",
            cancellationToken);
    }
}
