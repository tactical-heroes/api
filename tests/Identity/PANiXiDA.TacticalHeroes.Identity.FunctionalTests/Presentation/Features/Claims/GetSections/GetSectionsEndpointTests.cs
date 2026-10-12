using System.Text.Json;

using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Claims.GetSections;

using ApplicationClaimTypes = PANiXiDA.Core.Application.Authentication.ClaimTypes;
using SectionCatalog = PANiXiDA.TacticalHeroes.Common.Authentication.Sections.Sections;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Claims.GetSections;

public sealed class GetSectionsEndpointTests(FunctionalTestFixture fixture) : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "GET sections should return the shared catalog when authorized")]
    public async Task GetSections_Should_ReturnSharedCatalog_When_Authorized()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/claims/sections");

        using var response = await OAuthServiceAccessTokenTestHelper.SendAsync(Fixture, request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, body);
        using var document = JsonDocument.Parse(body);
        document.RootElement.EnumerateObject().Select(property => property.Name)
            .ShouldBe(["type", "items"], ignoreOrder: true);
        foreach (var item in document.RootElement.GetProperty("items").EnumerateArray())
        {
            item.EnumerateObject().Select(property => property.Name)
                .ShouldBe(["code", "displayName"], ignoreOrder: true);
        }

        var catalog = await response.Content.ReadFromJsonAsync<GetSectionsResponse>(JsonOptions, cancellationToken);
        catalog.ShouldNotBeNull();
        catalog.Type.ShouldBe(ApplicationClaimTypes.Section);
        catalog.Items.Count.ShouldBe(SectionCatalog.All.Count);
        foreach (var section in SectionCatalog.All)
        {
            catalog.Items.Single(item => item.Code == section.Code).DisplayName.ShouldBe(section.DisplayName);
        }
    }

    [Fact(DisplayName = "GET sections should return unauthorized when no token is provided")]
    public async Task GetSections_Should_ReturnUnauthorized_When_NoTokenIsProvided()
    {
        using var response = await Client.GetAsync("/api/v1/claims/sections", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
