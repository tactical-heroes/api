using System.Net.Http.Headers;
using System.Text.Json;

using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Claims.GetPermissions;

using ApplicationClaimTypes = PANiXiDA.Core.Application.Authentication.ClaimTypes;
using PermissionCatalog = PANiXiDA.TacticalHeroes.Common.Authentication.Permissions.Permissions;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Claims.GetPermissions;

public sealed class GetPermissionsEndpointTests(FunctionalTestFixture fixture) : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "GET permissions should return the shared catalog when authorized")]
    public async Task GetPermissions_Should_ReturnSharedCatalog_When_Authorized()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/claims/permissions");

        using var response = await OAuthServiceAccessTokenTestHelper.SendAsync(Fixture, request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, body);
        using var document = JsonDocument.Parse(body);
        document.RootElement.EnumerateObject().Select(property => property.Name)
            .ShouldBe(["type", "items"], ignoreOrder: true);
        document.RootElement.GetProperty("type").GetString().ShouldBe(ApplicationClaimTypes.Permission);
        foreach (var item in document.RootElement.GetProperty("items").EnumerateArray())
        {
            item.EnumerateObject().Select(property => property.Name).ShouldBe(
                ["code", "displayName", "moduleCode", "moduleDisplayName", "resourceCode", "resourceDisplayName"],
                ignoreOrder: true);
        }

        var permissions = await response.Content.ReadFromJsonAsync<GetPermissionsResponse>(
            JsonOptions,
            cancellationToken);
        permissions.ShouldNotBeNull();
        permissions.Type.ShouldBe(ApplicationClaimTypes.Permission);
        permissions.Items.Select(permission => permission.Code)
            .ShouldBe(PermissionCatalog.All.Select(permission => permission.Code));
        foreach (var permission in PermissionCatalog.All)
        {
            var item = permissions.Items.Single(item => item.Code == permission.Code);
            item.DisplayName.ShouldBe(permission.DisplayName);
            item.ModuleCode.ShouldBe(permission.ModuleCode);
            item.ResourceCode.ShouldBe(permission.ResourceCode);
            item.ResourceDisplayName.ShouldBe(permission.ResourceDisplayName);
        }
    }

    [Fact(DisplayName = "GET permissions should return unauthorized when no token is provided")]
    public async Task GetPermissions_Should_ReturnUnauthorized_When_NoTokenIsProvided()
    {
        using var response = await Client.GetAsync("/api/v1/claims/permissions", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Theory(DisplayName = "GET permissions should require edit permission when authenticated")]
    [InlineData("unrelated", HttpStatusCode.Forbidden)]
    [InlineData("identity.roles.update", HttpStatusCode.OK)]
    public async Task GetPermissions_Should_RequireEditPermission_When_Authenticated(
        string permission, HttpStatusCode expectedStatus)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/claims/permissions");
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await OAuthServiceAccessTokenTestHelper.IssueAccessTokenAsync(
                Fixture,
                [permission],
                cancellationToken));

        using var response = await Client.SendAsync(request, cancellationToken);

        response.StatusCode.ShouldBe(expectedStatus, await response.Content.ReadAsStringAsync(cancellationToken));
    }
}
