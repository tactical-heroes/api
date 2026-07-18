using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Login;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Auth.Login;

public sealed class LoginEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    private const string Password = "StrongPassword1!";

    [Fact(DisplayName = "POST auth login should authenticate a confirmed user from PostgreSQL")]
    public async Task PostLogin_Should_CreateAuthenticationSession_When_CredentialsAreValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await OAuthAuthorizationRequestTestHelper.CreateConfirmedUserAsync(
            Fixture,
            "login@example.test",
            "login-hero",
            Password,
            cancellationToken);
        using var client = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        var authorizePath = await OAuthAuthorizationRequestTestHelper.BuildAuthorizePathFromParAsync(
            client,
            OAuthAuthorizationRequestTestHelper.DefaultScopes,
            cancellationToken);

        using var response = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest(
                "login@example.test",
                Password,
                $"https://localhost{authorizePath}"),
            JsonOptions,
            cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        response.Headers.Location.ShouldBe(new Uri($"https://localhost{authorizePath}"));
        response.Headers.TryGetValues("Set-Cookie", out var cookies).ShouldBeTrue();
        cookies.ShouldContain(cookie =>
            cookie.Contains(".AspNetCore.Identity.Application=", StringComparison.Ordinal));
    }

    [Fact(DisplayName = "POST auth login should reject an invalid password")]
    public async Task PostLogin_Should_ReturnUnauthorized_When_PasswordIsInvalid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await OAuthAuthorizationRequestTestHelper.CreateConfirmedUserAsync(
            Fixture,
            "invalid-login@example.test",
            "invalid-login-hero",
            Password,
            cancellationToken);
        using var client = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        var authorizePath = await OAuthAuthorizationRequestTestHelper.BuildAuthorizePathFromParAsync(
            client,
            OAuthAuthorizationRequestTestHelper.DefaultScopes,
            cancellationToken);

        using var response = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest(
                "invalid-login@example.test",
                "WrongPassword1!",
                $"https://localhost{authorizePath}"),
            JsonOptions,
            cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
