using OpenIddict.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Login;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth.Logout;

public sealed class LogoutEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "GET OAuth logout should clear the application cookie and redirect to the SPA")]
    public async Task GetLogout_Should_ClearAuthenticationSession_When_UserIsLoggedIn()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await OAuthAuthorizationRequestTestHelper.CreateConfirmedUserAsync(
            Fixture,
            "logout@example.test",
            "logout-hero",
            "StrongPassword1!",
            cancellationToken);
        using var client = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        var authorizePath = await OAuthAuthorizationRequestTestHelper.BuildAuthorizePathFromParAsync(
            client,
            OAuthAuthorizationRequestTestHelper.DefaultScopes,
            cancellationToken);
        using var loginResponse = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest(
                "logout@example.test",
                "StrongPassword1!",
                $"https://localhost{authorizePath}"),
            JsonOptions,
            cancellationToken);

        loginResponse.StatusCode.ShouldBe(HttpStatusCode.Redirect);

        using var logoutResponse = await client.GetAsync(
            string.Concat(
                "/connect/logout",
                $"?{OpenIddictConstants.Parameters.ClientId}={OAuthAuthorizationRequestTestHelper.ClientId}",
                $"&{OpenIddictConstants.Parameters.PostLogoutRedirectUri}={Uri.EscapeDataString("https://localhost:5173/oauth/logout-callback")}",
                $"&{OpenIddictConstants.Parameters.State}=logout-state"),
            cancellationToken);
        var logoutResponseBody = await logoutResponse.Content.ReadAsStringAsync(cancellationToken);

        logoutResponse.StatusCode.ShouldBe(HttpStatusCode.Redirect, logoutResponseBody);
        logoutResponse.Headers.Location.ShouldNotBeNull();
        logoutResponse.Headers.Location.GetLeftPart(UriPartial.Path)
            .ShouldBe("https://localhost:5173/oauth/logout-callback");
        OAuthAuthorizationRequestTestHelper.GetQueryParameter(
                logoutResponse.Headers.Location,
                OpenIddictConstants.Parameters.State)
            .ShouldBe("logout-state");
        logoutResponse.Headers.TryGetValues("Set-Cookie", out var cookies).ShouldBeTrue();
        cookies.ShouldContain(cookie =>
            cookie.Contains(".AspNetCore.Identity.Application=;", StringComparison.Ordinal));

        var nextAuthorizePath = await OAuthAuthorizationRequestTestHelper.BuildAuthorizePathFromParAsync(
            client,
            OAuthAuthorizationRequestTestHelper.DefaultScopes,
            cancellationToken);
        using var authorizeResponse = await client.GetAsync(nextAuthorizePath, cancellationToken);

        authorizeResponse.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        authorizeResponse.Headers.Location.ShouldNotBeNull();
        authorizeResponse.Headers.Location.GetLeftPart(UriPartial.Path).ShouldBe("https://localhost:5173/login");
    }
}
