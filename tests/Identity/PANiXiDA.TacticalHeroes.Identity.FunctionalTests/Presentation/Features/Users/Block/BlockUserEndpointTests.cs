using System.Net.Http.Headers;

using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Users.Block;

public sealed class BlockUserEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "POST user block should persist blocked status and revoke access when user exists")]
    public async Task BlockUser_Should_PersistBlockedStatusAndRevokeAccess_When_UserExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new UserApiTestClient(Fixture);
        var createdUser = await client.CreateAsync(
            UserApiTestClient.CreateDefaultRequest() with
            {
                Claims = [new("permission", "identity.users.read")]
            },
            cancellationToken);
        using var userClient = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        var tokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            userClient,
            "hero@example.com",
            "StrongPassword1!",
            cancellationToken);
        userClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        using var initialResponse = await userClient.GetAsync($"/api/v1/users/{createdUser.Id}", cancellationToken);
        initialResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        await client.BlockAsync(createdUser.Id, cancellationToken);
        var user = await UserDatabaseTestHelper.FindAsync(
            Fixture,
            createdUser.Id,
            cancellationToken);

        user.ShouldNotBeNull();
        user.Status.ShouldBe("Blocked");
        using var response = await userClient.GetAsync($"/api/v1/users/{createdUser.Id}", cancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "POST user block should keep sessions revoked when user is unblocked")]
    public async Task BlockUser_Should_KeepSessionsRevoked_When_UserIsUnblocked()
    {
        var ct = TestContext.Current.CancellationToken;
        var api = new UserApiTestClient(Fixture);
        var user = await api.CreateAsync(request: null, cancellationToken: ct);
        var unrelatedUser = await api.CreateAsync(
            UserApiTestClient.CreateDefaultRequest("unrelated@example.test", "unrelated"), ct);
        using var current = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        using var other = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        using var unrelated = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        var currentTokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            current, "hero@example.com", "StrongPassword1!", ct);
        var otherTokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            other, "hero@example.com", "StrongPassword1!", ct);
        var unrelatedTokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            unrelated, "unrelated@example.test", "StrongPassword1!", ct);
        var before = await UserDatabaseTestHelper.FindAsync(Fixture, user.Id, ct);
        var unrelatedBefore = await UserDatabaseTestHelper.FindAsync(Fixture, unrelatedUser.Id, ct);

        await api.BlockAsync(user.Id, ct);
        var blocked = await UserDatabaseTestHelper.FindAsync(Fixture, user.Id, ct);
        await api.UnblockAsync(user.Id, ct);

        var active = await UserDatabaseTestHelper.FindAsync(Fixture, user.Id, ct);
        var unrelatedAfter = await UserDatabaseTestHelper.FindAsync(Fixture, unrelatedUser.Id, ct);
        blocked!.SecurityStamp.ShouldNotBe(before!.SecurityStamp);
        active!.Status.ShouldBe("Active");
        active.SecurityStamp.ShouldBe(blocked.SecurityStamp);
        unrelatedAfter!.SecurityStamp.ShouldBe(unrelatedBefore!.SecurityStamp);
        await AssertAccessAsync(current, currentTokens.AccessToken, HttpStatusCode.Unauthorized, ct);
        await AssertAccessAsync(other, otherTokens.AccessToken, HttpStatusCode.Unauthorized, ct);
        await AssertAccessAsync(unrelated, unrelatedTokens.AccessToken, HttpStatusCode.OK, ct);
        await AssertCookieAsync(current, false, ct);
        await AssertCookieAsync(other, false, ct);
        await AssertCookieAsync(unrelated, true, ct);
        using var currentRefresh = await RefreshAsync(current, currentTokens.RefreshToken, ct);
        currentRefresh.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        using var otherRefresh = await RefreshAsync(other, otherTokens.RefreshToken, ct);
        otherRefresh.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        using var unrelatedRefresh = await RefreshAsync(unrelated, unrelatedTokens.RefreshToken, ct);
        unrelatedRefresh.StatusCode.ShouldBe(HttpStatusCode.OK);
        var newTokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            current, "hero@example.com", "StrongPassword1!", ct);
        await AssertAccessAsync(current, newTokens.AccessToken, HttpStatusCode.OK, ct);
    }

    private static async Task AssertAccessAsync(
        HttpClient client, string accessToken, HttpStatusCode expected, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/connect/userinfo");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await client.SendAsync(request, ct);
        response.StatusCode.ShouldBe(expected);
    }

    private static async Task AssertCookieAsync(HttpClient client, bool valid, CancellationToken ct)
    {
        var path = await OAuthAuthorizationRequestTestHelper.BuildAuthorizePathFromParAsync(
            client: client,
            scopes: OAuthAuthorizationRequestTestHelper.DefaultScopes,
            redirectUri: OAuthAuthorizationRequestTestHelper.RedirectUri,
            cancellationToken: ct);
        using var response = await client.GetAsync(path, ct);
        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        var callback = response.Headers.Location!.GetLeftPart(UriPartial.Path);
        callback.ShouldBe(valid
            ? OAuthAuthorizationRequestTestHelper.RedirectUri
            : "https://localhost:5173/login");
    }

    private static async Task<HttpResponseMessage> RefreshAsync(HttpClient client, string token, CancellationToken ct)
    {
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = OAuthAuthorizationRequestTestHelper.ClientId,
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = token
        });
        return await client.PostAsync("/connect/token", content, ct);
    }
}
