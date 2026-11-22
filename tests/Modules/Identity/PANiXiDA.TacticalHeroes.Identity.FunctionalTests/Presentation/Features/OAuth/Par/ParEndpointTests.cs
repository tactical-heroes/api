namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth.Par;

[Collection(FunctionalTestCollectionDefinition.Name)]
public sealed class ParEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "POST OAuth PAR should persist a pushed authorization request when request is valid")]
    public async Task PostPar_Should_ReturnRequestUri_When_RequestIsValid()
    {
        using var client = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);

        var requestUri = await OAuthAuthorizationRequestTestHelper.PushAuthorizationRequestAsync(
            client: client,
            scopes: OAuthAuthorizationRequestTestHelper.DefaultScopes,
            redirectUri: "https://localhost:5173/oauth/callback",
            cancellationToken: TestContext.Current.CancellationToken);

        requestUri.ShouldNotBeNullOrWhiteSpace();
        requestUri.ShouldStartWith("urn:ietf:params:oauth:request_uri:");
    }
}
