using System.Text.Json;

using OpenIddict.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth.Introspect;

public sealed class IntrospectEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "POST OAuth introspect should report a persisted access token as active")]
    public async Task PostIntrospect_Should_ReturnActiveToken_When_AccessTokenIsValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var createdUser = await OAuthAuthorizationRequestTestHelper.CreateConfirmedUserAsync(
            Fixture,
            "introspect@example.test",
            "introspect-hero",
            "StrongPassword1!",
            cancellationToken);
        using var client = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        var tokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            client,
            "introspect@example.test",
            "StrongPassword1!",
            cancellationToken);

        using var response = await client.PostAsync(
            "/connect/introspect",
            new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    [OpenIddictConstants.Parameters.ClientId] = OAuthAuthorizationRequestTestHelper.ClientId,
                    [OpenIddictConstants.Parameters.Token] = tokens.AccessToken,
                    [OpenIddictConstants.Parameters.TokenTypeHint] = OpenIddictConstants.TokenTypeHints.AccessToken
                }),
            cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, responseBody);
        using var document = JsonDocument.Parse(responseBody);
        document.RootElement.GetProperty("active").GetBoolean().ShouldBeTrue();
        document.RootElement.GetProperty(OpenIddictConstants.Claims.Subject).GetString()
            .ShouldBe(createdUser.Id.ToString());
    }
}
