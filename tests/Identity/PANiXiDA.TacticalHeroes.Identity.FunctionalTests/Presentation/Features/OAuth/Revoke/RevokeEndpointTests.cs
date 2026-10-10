using System.Net.Http.Headers;

using OpenIddict.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth.Revoke;

public sealed class RevokeEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Theory(DisplayName = "POST OAuth revoke should invalidate a persisted access token when a protected endpoint is requested")]
    [InlineData("/connect/userinfo")]
    [InlineData("/api/v1/users")]
    public async Task PostRevoke_Should_RejectRevokedToken_When_AProtectedEndpointIsRequested(string path)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await OAuthAuthorizationRequestTestHelper.CreateConfirmedUserAsync(
            Fixture,
            "revoke@example.test",
            "revoke-hero",
            "StrongPassword1!",
            cancellationToken);
        using var client = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        var tokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            client,
            "revoke@example.test",
            "StrongPassword1!",
            cancellationToken);

        using var validRequest = new HttpRequestMessage(HttpMethod.Get, path);
        validRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        using var validResponse = await client.SendAsync(validRequest, cancellationToken);
        validResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        using var revokeResponse = await client.PostAsync(
            "/connect/revoke",
            new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    [OpenIddictConstants.Parameters.ClientId] = OAuthAuthorizationRequestTestHelper.ClientId,
                    [OpenIddictConstants.Parameters.Token] = tokens.AccessToken,
                    [OpenIddictConstants.Parameters.TokenTypeHint] = OpenIddictConstants.TokenTypeHints.AccessToken
                }),
            cancellationToken);
        var revokeResponseBody = await revokeResponse.Content.ReadAsStringAsync(cancellationToken);

        revokeResponse.StatusCode.ShouldBe(HttpStatusCode.OK, revokeResponseBody);

        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        using var userInfoResponse = await client.SendAsync(request, cancellationToken);

        userInfoResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
