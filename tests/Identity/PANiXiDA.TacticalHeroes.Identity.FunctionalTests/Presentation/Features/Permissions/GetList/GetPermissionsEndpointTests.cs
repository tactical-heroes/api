using System.Text.Json;

using PANiXiDA.TacticalHeroes.Common.Authentication.Sections;
using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Permissions.GetList;

using PermissionCatalog = PANiXiDA.TacticalHeroes.Common.Authentication.Permissions.Permissions;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Permissions.GetList;

public sealed class GetPermissionsEndpointTests(FunctionalTestFixture fixture) : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "GET permissions should return the shared catalog when authorized")]
    public async Task GetPermissions_Should_ReturnSharedCatalog_When_Authorized()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/permissions");

        using var response = await OAuthServiceAccessTokenTestHelper.SendAsync(Fixture, request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, body);
        using var document = JsonDocument.Parse(body);
        foreach (var item in document.RootElement.EnumerateArray())
        {
            item.EnumerateObject().Select(property => property.Name).ShouldBe(
                ["code", "displayName", "sectionCode", "sectionDisplayName", "resourceCode", "resourceDisplayName"],
                ignoreOrder: true);
        }

        var permissions = await response.Content.ReadFromJsonAsync<PermissionResponse[]>(
            JsonOptions,
            cancellationToken);
        permissions.ShouldNotBeNull();
        permissions.Length.ShouldBe(PermissionCatalog.All.Count);
        foreach (var permission in PermissionCatalog.All)
        {
            var item = permissions.Single(item => item.Code == permission.Code);
            item.DisplayName.ShouldBe(permission.DisplayName);
            item.SectionCode.ShouldBe(permission.SectionCode);
            item.SectionDisplayName.ShouldBe(Sections.All
                .Single(section => section.Code == permission.SectionCode)
                .DisplayName);
            item.ResourceCode.ShouldBe(permission.ResourceCode);
            item.ResourceDisplayName.ShouldBe(permission.ResourceDisplayName);
        }
    }

    [Fact(DisplayName = "GET permissions should return unauthorized when no token is provided")]
    public async Task GetPermissions_Should_ReturnUnauthorized_When_NoTokenIsProvided()
    {
        using var response = await Client.GetAsync("/api/v1/permissions", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
