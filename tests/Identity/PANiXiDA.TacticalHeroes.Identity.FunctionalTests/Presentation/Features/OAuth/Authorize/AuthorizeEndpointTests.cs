namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth.Authorize;

public sealed class AuthorizeEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Theory(DisplayName = "GET OAuth authorize should redirect an anonymous user to the configured login page")]
    [InlineData(OAuthAuthorizationRequestTestHelper.RedirectUri)]
    [InlineData("https://dev.tactical-heroes.panixida.ru/oauth/callback")]
    public async Task GetAuthorize_Should_RedirectToConfiguredLogin_When_UserIsAnonymous(
        string redirectUri)
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
        response.Headers.Location.GetLeftPart(UriPartial.Path).ShouldBe("https://localhost:5173/login");
        response.Headers.Location.Query.ShouldContain(
            $"returnUrl={Uri.EscapeDataString($"https://localhost{authorizePath}")}");
    }
}
