using System.Net.Http.Headers;

using OpenIddict.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth.Revoke;

public sealed class RevokeEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "POST OAuth revoke should invalidate a persisted access token")]
    public async Task PostRevoke_Should_RejectRevokedToken_When_UserInfoIsRequested()
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

        using var request = new HttpRequestMessage(HttpMethod.Get, "/connect/userinfo");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        using var userInfoResponse = await client.SendAsync(request, cancellationToken);

        userInfoResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
