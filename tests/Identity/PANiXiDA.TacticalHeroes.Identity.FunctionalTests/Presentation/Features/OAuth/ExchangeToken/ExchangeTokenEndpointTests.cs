using System.Text.Json;

using OpenIddict.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth.ExchangeToken;

public sealed class ExchangeTokenEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "POST OAuth token should exchange and refresh a persisted user authorization")]
    public async Task PostToken_Should_IssueNewTokens_When_RefreshTokenIsValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await OAuthAuthorizationRequestTestHelper.CreateConfirmedUserAsync(
            Fixture,
            "exchange@example.test",
            "exchange-hero",
            "StrongPassword1!",
            cancellationToken);
        using var client = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        var tokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            client,
            "exchange@example.test",
            "StrongPassword1!",
            cancellationToken);

        using var response = await client.PostAsync(
            "/connect/token",
            new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    [OpenIddictConstants.Parameters.GrantType] = OpenIddictConstants.GrantTypes.RefreshToken,
                    [OpenIddictConstants.Parameters.ClientId] = OAuthAuthorizationRequestTestHelper.ClientId,
                    [OpenIddictConstants.Parameters.RefreshToken] = tokens.RefreshToken
                }),
            cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, responseBody);
        using var document = JsonDocument.Parse(responseBody);
        document.RootElement.GetProperty("access_token").GetString().ShouldNotBeNullOrWhiteSpace();
        document.RootElement.GetProperty("refresh_token").GetString().ShouldNotBeNullOrWhiteSpace();
    }
}
