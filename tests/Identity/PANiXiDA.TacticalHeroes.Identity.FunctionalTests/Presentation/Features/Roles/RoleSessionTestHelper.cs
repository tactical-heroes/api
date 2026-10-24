using System.Net.Http.Headers;
using System.Text.Json;

using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth;
using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Users;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.GetUserInfo;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.Create;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Roles;

internal sealed class RoleSessionTestHelper(FunctionalTestFixture fixture) : IDisposable
{
    internal const string Password = "StrongPassword1!";
    internal const string Email = "role-member@example.test";

    internal HttpClient Current { get; } = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(fixture);
    internal HttpClient Other { get; } = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(fixture);
    internal HttpClient Member { get; } = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(fixture);
    internal HttpClient Unrelated { get; } = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(fixture);
    internal Guid RoleId { get; private set; }
    internal Guid UserId { get; private set; }
    internal OAuthTokenResponse CurrentTokens { get; private set; } = null!;
    internal OAuthTokenResponse OtherTokens { get; private set; } = null!;
    internal OAuthTokenResponse MemberTokens { get; private set; } = null!;
    internal OAuthTokenResponse UnrelatedTokens { get; private set; } = null!;

    public void Dispose()
    {
        Current.Dispose();
        Other.Dispose();
        Member.Dispose();
        Unrelated.Dispose();
    }

    internal static async Task<HttpResponseMessage> RefreshAsync(
        HttpClient client, string refreshToken, CancellationToken ct)
    {
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["client_id"] = OAuthAuthorizationRequestTestHelper.ClientId,
            ["refresh_token"] = refreshToken
        });
        return await client.PostAsync("/connect/token", content, ct);
    }

    internal static async Task AssertAccessAsync(
        HttpClient client,
        string token,
        HttpStatusCode status,
        string path,
        CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var response = await client.SendAsync(request, ct);
        response.StatusCode.ShouldBe(status, await response.Content.ReadAsStringAsync(ct));
    }

    internal static async Task AssertCookieAsync(HttpClient client, bool valid, CancellationToken ct)
    {
        var path = await OAuthAuthorizationRequestTestHelper.BuildAuthorizePathFromParAsync(
            client: client,
            scopes: OAuthAuthorizationRequestTestHelper.DefaultScopes,
            redirectUri: "https://localhost:5173/oauth/callback",
            cancellationToken: ct);
        using var response = await client.GetAsync(path, ct);
        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        response.Headers.Location!.AbsoluteUri.ShouldStartWith(valid
            ? OAuthAuthorizationRequestTestHelper.RedirectUri
            : "https://localhost:5173/login");
    }

    internal async Task InitializeAsync(bool hasPermission, CancellationToken ct)
    {
        var role = await new RoleApiTestClient(fixture).CreateAsync(
            new CreateRoleRequest(
                "SessionRole",
                false,
                hasPermission ? [new("permission", "identity.roles.read")] : []),
            ct);
        RoleId = role.Id;
        var users = new UserApiTestClient(fixture);
        foreach (var (email, name, assigned) in new[]
                 {
                     (Email, "role-member", true),
                     ("second-member@example.test", "second-member", true),
                     ("unrelated@example.test", "unrelated", false)
                 })
        {
            var user = await users.CreateAsync(
                UserApiTestClient.CreateDefaultRequest(email, name) with
                {
                    RoleIds = assigned ? [RoleId] : [],
                    Claims = [new("permission", "identity.roles.update"), new("permission", "identity.roles.delete")]
                },
                ct);
            if (email == Email)
            {
                UserId = user.Id;
            }
        }

        CurrentTokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(Current, Email, Password, ct);
        OtherTokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(Other, Email, Password, ct);
        MemberTokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            Member, "second-member@example.test", Password, ct);
        UnrelatedTokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            Unrelated, "unrelated@example.test", Password, ct);
    }

    internal Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        bool self,
        bool service,
        CancellationToken ct)
    {
        if (service)
        {
            return OAuthServiceAccessTokenTestHelper.SendAsync(fixture, request, ct);
        }

        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer", self ? CurrentTokens.AccessToken : UnrelatedTokens.AccessToken);
        return (self ? Current : Unrelated).SendAsync(request, ct);
    }

    internal async Task AssertRevokedAsync(
        HttpResponseMessage response, bool self, bool hasPermission, string? roleName, CancellationToken ct)
    {
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent, await response.Content.ReadAsStringAsync(ct));
        response.Headers.Contains("X-Session-Refresh-Required").ShouldBe(self);
        if (self)
        {
            response.Headers.GetValues("X-Session-Refresh-Required").ShouldBe(["true"]);
            response.Headers.Contains("Set-Cookie").ShouldBeTrue();
        }
        await AssertAccessAsync(
            client: Current,
            token: CurrentTokens.AccessToken,
            status: HttpStatusCode.Unauthorized,
            path: "/connect/userinfo",
            ct: ct);
        await AssertAccessAsync(
            client: Other,
            token: OtherTokens.AccessToken,
            status: HttpStatusCode.Unauthorized,
            path: "/connect/userinfo",
            ct: ct);
        await AssertAccessAsync(
            client: Member,
            token: MemberTokens.AccessToken,
            status: HttpStatusCode.Unauthorized,
            path: "/connect/userinfo",
            ct: ct);
        await AssertAccessAsync(
            client: Unrelated,
            token: UnrelatedTokens.AccessToken,
            status: HttpStatusCode.OK,
            path: "/connect/userinfo",
            ct: ct);
        await AssertCookieAsync(Current, self, ct);
        await AssertCookieAsync(Other, false, ct);
        await AssertCookieAsync(Member, false, ct);
        await AssertCookieAsync(Unrelated, true, ct);
        using var otherRefresh = await RefreshAsync(Other, OtherTokens.RefreshToken, ct);
        otherRefresh.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        using var memberRefresh = await RefreshAsync(Member, MemberTokens.RefreshToken, ct);
        memberRefresh.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        using var refresh = await RefreshAsync(Current, CurrentTokens.RefreshToken, ct);
        refresh.StatusCode.ShouldBe(self ? HttpStatusCode.OK : HttpStatusCode.BadRequest);
        if (self)
        {
            using var document = JsonDocument.Parse(await refresh.Content.ReadAsStringAsync(ct));
            var access = document.RootElement.GetProperty("access_token").GetString()!;
            await AssertAccessAsync(
                Current,
                access,
                hasPermission ? HttpStatusCode.OK : HttpStatusCode.Forbidden,
                "/api/v1/roles",
                ct);
            using var request = new HttpRequestMessage(HttpMethod.Get, "/connect/userinfo");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", access);
            using var userInfo = await Current.SendAsync(request, ct);
            userInfo.StatusCode.ShouldBe(HttpStatusCode.OK);
            var user = await userInfo.Content.ReadFromJsonAsync<GetUserInfoResponse>(cancellationToken: ct);
            user.ShouldNotBeNull();
            (user.Role ?? []).ShouldBe(roleName is null ? [] : [roleName]);
            using var next = await RefreshAsync(
                Current, document.RootElement.GetProperty("refresh_token").GetString()!, ct);
            next.StatusCode.ShouldBe(HttpStatusCode.OK);
        }
    }
}
