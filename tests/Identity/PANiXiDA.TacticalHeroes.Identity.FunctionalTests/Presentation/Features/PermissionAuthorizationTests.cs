using System.Net.Http.Headers;

using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.Create;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features;

public sealed class PermissionAuthorizationTests(FunctionalTestFixture fixture) : FunctionalTestBase(fixture)
{
    [Theory(DisplayName = "Get should require permission when authenticated")]
    [InlineData("/api/v1/users?pageNumber=1&pageSize=20", "identity.users.read")]
    [InlineData("/api/v1/roles?pageNumber=1&pageSize=20", "identity.roles.read")]
    [InlineData("/api/v1/users/statuses", "identity.users.read")]
    [InlineData("/api/v1/claims/permissions", "identity.roles.update")]
    public async Task Get_Should_RequirePermission_When_Authenticated(string path, string permission)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var denied = new HttpRequestMessage(HttpMethod.Get, path);
        denied.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await OAuthServiceAccessTokenTestHelper.IssueAccessTokenAsync(Fixture, cancellationToken, ["unrelated"]));
        using var deniedResponse = await Client.SendAsync(denied, cancellationToken);
        deniedResponse.StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        using var allowed = new HttpRequestMessage(HttpMethod.Get, path);
        allowed.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await OAuthServiceAccessTokenTestHelper.IssueAccessTokenAsync(Fixture, cancellationToken, [permission]));
        using var allowedResponse = await Client.SendAsync(allowed, cancellationToken);
        allowedResponse.StatusCode.ShouldBe(
            HttpStatusCode.OK,
            await allowedResponse.Content.ReadAsStringAsync(cancellationToken));
    }

    [Theory(DisplayName = "Get should return forbidden when only form permission is granted")]
    [InlineData("/api/v1/roles?pageNumber=1&pageSize=20", "identity.users.create")]
    [InlineData("/api/v1/roles?pageNumber=1&pageSize=20", "identity.users.update")]
    [InlineData("/api/v1/users/statuses", "identity.users.create")]
    [InlineData("/api/v1/users/statuses", "identity.users.update")]
    public async Task Get_Should_ReturnForbidden_When_OnlyFormPermissionIsGranted(string path, string permission)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await OAuthServiceAccessTokenTestHelper.IssueAccessTokenAsync(Fixture, cancellationToken, [permission]));

        using var response = await Client.SendAsync(request, cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Theory(DisplayName = "Post should enforce permission and claim contract when creating role")]
    [InlineData("identity.roles.read", "custom", HttpStatusCode.Forbidden)]
    [InlineData("identity.roles.create", "section", HttpStatusCode.BadRequest)]
    [InlineData("identity.roles.create", " SECTION ", HttpStatusCode.BadRequest)]
    [InlineData("identity.roles.create", "custom", HttpStatusCode.Created)]
    public async Task Post_Should_EnforcePermissionAndClaimContract_When_CreatingRole(
        string permission, string claimType, HttpStatusCode expected)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/roles")
        {
            Content = JsonContent.Create(
                new CreateRoleRequest(
                    "PermissionTest", false, [new(claimType, "identity")]),
                options: JsonOptions)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await OAuthServiceAccessTokenTestHelper.IssueAccessTokenAsync(Fixture, cancellationToken, [permission]));

        using var response = await Client.SendAsync(request, cancellationToken);

        response.StatusCode.ShouldBe(expected, await response.Content.ReadAsStringAsync(cancellationToken));
    }
}
