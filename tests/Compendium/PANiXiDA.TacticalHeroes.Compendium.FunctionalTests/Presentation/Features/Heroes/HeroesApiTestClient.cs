using PANiXiDA.TacticalHeroes.Compendium.FunctionalTests.Presentation.Features.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Factions.Create;
using PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Heroes.Create;
using PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Heroes.GetDetails;
using PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Heroes.GetList;
using PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Heroes.Update;

namespace PANiXiDA.TacticalHeroes.Compendium.FunctionalTests.Presentation.Features.Heroes;

internal sealed class HeroesApiTestClient(FunctionalTestFixture fixture)
{
    private const string HeroesPath = "/api/v1/heroes";

    internal Task<CreateFactionResponse> CreateFactionAsync(
        CancellationToken cancellationToken,
        CreateFactionRequest? request = null)
    {
        return new FactionsApiTestClient(fixture).CreateAsync(
            cancellationToken,
            request);
    }

    internal async Task<CreateHeroResponse> CreateAsync(
        Guid factionId,
        CancellationToken cancellationToken,
        CreateHeroRequest? request = null)
    {
        request ??= CreateRequest(
            factionId: factionId,
            name: "Orrin");
        using var response = await fixture.Client.PostAsJsonAsync(
            HeroesPath,
            request,
            TestJsonSerializerOptions.Web,
            cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Created, responseBody);
        response.Headers.Location.ShouldNotBeNull();

        return await response.Content.ReadFromJsonAsync<CreateHeroResponse>(
                TestJsonSerializerOptions.Web,
                cancellationToken)
            ?? throw new InvalidOperationException("Created hero was not returned.");
    }

    internal async Task<GetHeroDetailsResponse> GetDetailsAsync(
        Guid heroId,
        CancellationToken cancellationToken)
    {
        using var response = await fixture.Client.GetAsync(
            $"{HeroesPath}/{heroId}",
            cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, responseBody);

        return await response.Content.ReadFromJsonAsync<GetHeroDetailsResponse>(
                TestJsonSerializerOptions.Web,
                cancellationToken)
            ?? throw new InvalidOperationException("Hero details were not returned.");
    }

    internal async Task<PaginationResult<HeroListItemResponse>> GetListAsync(
        CancellationToken cancellationToken)
    {
        using var response = await fixture.Client.GetAsync(
            $"{HeroesPath}?pageNumber=1&pageSize=20",
            cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, responseBody);

        return await response.Content.ReadFromJsonAsync<
                PaginationResult<HeroListItemResponse>>(
                TestJsonSerializerOptions.Web,
                cancellationToken)
            ?? throw new InvalidOperationException("Hero page was not returned.");
    }

    internal async Task UpdateAsync(
        Guid heroId,
        UpdateHeroRequest request,
        CancellationToken cancellationToken)
    {
        using var response = await fixture.Client.PutAsJsonAsync(
            $"{HeroesPath}/{heroId}",
            request,
            TestJsonSerializerOptions.Web,
            cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent, responseBody);
    }

    internal async Task DeleteAsync(
        Guid heroId,
        CancellationToken cancellationToken)
    {
        using var response = await fixture.Client.DeleteAsync(
            $"{HeroesPath}/{heroId}",
            cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent, responseBody);
    }

    internal Task<HttpResponseMessage> GetDetailsResponseAsync(
        Guid heroId,
        CancellationToken cancellationToken)
    {
        return fixture.Client.GetAsync(
            $"{HeroesPath}/{heroId}",
            cancellationToken);
    }

    internal static CreateHeroRequest CreateRequest(
        Guid factionId,
        string name)
    {
        return new CreateHeroRequest(
            Name: name,
            Description: $"{name} description.",
            Attack: 8,
            Defense: 6,
            MinimumDamage: 3,
            MaximumDamage: 7,
            Initiative: 10.5,
            Morale: 4,
            Luck: 2,
            FactionId: factionId);
    }
}
