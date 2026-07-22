namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth.Authorize;

public sealed class AuthorizeEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Theory(DisplayName = "GET OAuth authorize should redirect an anonymous user to the client login page")]
    [InlineData(
        OAuthAuthorizationRequestTestHelper.RedirectUri,
        "https://localhost:5173/login")]
    [InlineData(
        "https://dev.tactical-heroes.panixida.ru/oauth/callback",
        "https://dev.tactical-heroes.panixida.ru/login")]
    public async Task GetAuthorize_Should_RedirectToClientLogin_When_UserIsAnonymous(
        string redirectUri,
        string expectedLoginUrl)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        var authorizePath = await OAuthAuthorizationRequestTestHelper.BuildAuthorizePathFromParAsync(
            client,
            OAuthAuthorizationRequestTestHelper.DefaultScopes,
            cancellationToken,
            redirectUri);

        using var response = await client.GetAsync(authorizePath, cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location.GetLeftPart(UriPartial.Path).ShouldBe(expectedLoginUrl);
        response.Headers.Location.Query.ShouldContain(
            $"returnUrl={Uri.EscapeDataString($"https://localhost{authorizePath}")}");
    }
}
