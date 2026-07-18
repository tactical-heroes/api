using System.Text.Json;

using Microsoft.AspNetCore.Mvc.Testing;

using OpenIddict.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Users;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Login;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Create;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth;

internal static class OAuthAuthorizationRequestTestHelper
{
    internal const string ClientId = "tactical-heroes-web";
    internal const string CodeChallenge = "E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM";
    internal const string CodeVerifier = "dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk";
    internal const string ParPath = "/connect/par";
    internal const string RedirectUri = "https://localhost:5173/oauth/callback";

    private const string LoginPath = "/api/v1/auth/login";
    private const string TokenPath = "/connect/token";

    internal static HttpClient CreateOAuthClient(FunctionalTestFixture fixture)
    {
        return fixture.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                BaseAddress = new Uri("https://localhost")
            });
    }

    internal static async Task<CreateUserResponse> CreateConfirmedUserAsync(
        FunctionalTestFixture fixture,
        string email,
        string userName,
        string password,
        CancellationToken cancellationToken)
    {
        var api = new UserApiTestClient(fixture);

        return await api.CreateAsync(
            cancellationToken,
            UserApiTestClient.CreateDefaultRequest(
                email,
                userName,
                isConfirmed: true) with
            {
                Password = password
            });
    }

    internal static async Task<string> BuildAuthorizePathFromParAsync(
        HttpClient client,
        IReadOnlyCollection<string> scopes,
        CancellationToken cancellationToken)
    {
        var requestUri = await PushAuthorizationRequestAsync(
            client,
            scopes,
            cancellationToken);

        return BuildAuthorizePath(requestUri);
    }

    internal static async Task<string> PushAuthorizationRequestAsync(
        HttpClient client,
        IReadOnlyCollection<string> scopes,
        CancellationToken cancellationToken)
    {
        using var response = await client.PostAsync(
            ParPath,
            new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    [OpenIddictConstants.Parameters.ResponseType] = OpenIddictConstants.ResponseTypes.Code,
                    [OpenIddictConstants.Parameters.ClientId] = ClientId,
                    [OpenIddictConstants.Parameters.RedirectUri] = RedirectUri,
                    [OpenIddictConstants.Parameters.Scope] = string.Join(' ', scopes),
                    [OpenIddictConstants.Parameters.State] = "functional-test-state",
                    [OpenIddictConstants.Parameters.CodeChallenge] = CodeChallenge,
                    [OpenIddictConstants.Parameters.CodeChallengeMethod] = OpenIddictConstants.CodeChallengeMethods.Sha256
                }),
            cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Created, responseBody);
        using var document = JsonDocument.Parse(responseBody);

        return document.RootElement.GetProperty("request_uri").GetString()
            ?? throw new InvalidOperationException("Pushed authorization request URI was not returned.");
    }

    internal static string BuildAuthorizePath(string requestUri)
    {
        return string.Concat(
            "/connect/authorize",
            $"?client_id={ClientId}",
            $"&request_uri={Uri.EscapeDataString(requestUri)}");
    }

    internal static async Task<OAuthTokenResponse> IssueUserTokensAsync(
        HttpClient client,
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        var authorizationCode = await LoginAndGetAuthorizationCodeAsync(
            client,
            email,
            password,
            cancellationToken);
        using var response = await client.PostAsync(
            TokenPath,
            new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    [OpenIddictConstants.Parameters.GrantType] = OpenIddictConstants.GrantTypes.AuthorizationCode,
                    [OpenIddictConstants.Parameters.ClientId] = ClientId,
                    [OpenIddictConstants.Parameters.Code] = authorizationCode,
                    [OpenIddictConstants.Parameters.CodeVerifier] = CodeVerifier,
                    [OpenIddictConstants.Parameters.RedirectUri] = RedirectUri
                }),
            cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, responseBody);
        using var document = JsonDocument.Parse(responseBody);
        var root = document.RootElement;

        return new OAuthTokenResponse(
            root.GetProperty("access_token").GetString()
                ?? throw new InvalidOperationException("Access token was not returned."),
            root.GetProperty("refresh_token").GetString()
                ?? throw new InvalidOperationException("Refresh token was not returned."));
    }

    internal static async Task<string> LoginAndGetAuthorizationCodeAsync(
        HttpClient client,
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        var authorizePath = await BuildAuthorizePathFromParAsync(
            client,
            DefaultScopes,
            cancellationToken);
        using var loginResponse = await client.PostAsJsonAsync(
            LoginPath,
            new LoginRequest(email, password, $"https://localhost{authorizePath}"),
            TestJsonSerializerOptions.Web,
            cancellationToken);
        var loginResponseBody = await loginResponse.Content.ReadAsStringAsync(cancellationToken);

        loginResponse.StatusCode.ShouldBe(HttpStatusCode.Redirect, loginResponseBody);

        using var authorizeResponse = await client.GetAsync(authorizePath, cancellationToken);
        var authorizeResponseBody = await authorizeResponse.Content.ReadAsStringAsync(cancellationToken);

        authorizeResponse.StatusCode.ShouldBe(HttpStatusCode.Redirect, authorizeResponseBody);
        authorizeResponse.Headers.Location.ShouldNotBeNull();
        authorizeResponse.Headers.Location.GetLeftPart(UriPartial.Path).ShouldBe(RedirectUri);

        return GetQueryParameter(authorizeResponse.Headers.Location, OpenIddictConstants.Parameters.Code);
    }

    internal static string GetQueryParameter(Uri uri, string name)
    {
        foreach (var parameter in uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = parameter.Split('=', 2);

            if (parts.Length == 2 &&
                string.Equals(Uri.UnescapeDataString(parts[0]), name, StringComparison.Ordinal))
            {
                return Uri.UnescapeDataString(parts[1].Replace("+", " ", StringComparison.Ordinal));
            }
        }

        throw new InvalidOperationException($"Query parameter '{name}' was not found.");
    }

    internal static IReadOnlyCollection<string> DefaultScopes { get; } =
    [
        OpenIddictConstants.Scopes.OpenId,
        OpenIddictConstants.Scopes.OfflineAccess,
        OpenIddictConstants.Scopes.Profile,
        OpenIddictConstants.Scopes.Email,
        OpenIddictConstants.Scopes.Roles
    ];
}

internal sealed record OAuthTokenResponse(
    string AccessToken,
    string RefreshToken);
