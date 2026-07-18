namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth.Par;

public sealed class ParEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "POST OAuth PAR should persist a pushed authorization request")]
    public async Task PostPar_Should_ReturnRequestUri_When_RequestIsValid()
    {
        using var client = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);

        var requestUri = await OAuthAuthorizationRequestTestHelper.PushAuthorizationRequestAsync(
            client,
            OAuthAuthorizationRequestTestHelper.DefaultScopes,
            TestContext.Current.CancellationToken);

        requestUri.ShouldNotBeNullOrWhiteSpace();
        requestUri.ShouldStartWith("urn:ietf:params:oauth:request_uri:");
    }
}
