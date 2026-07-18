using System.Text.Json;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth.Discovery;

public sealed class DiscoveryEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "GET OpenID configuration should expose the configured Identity endpoints")]
    public async Task GetOpenIdConfiguration_Should_ReturnConfiguredEndpoints()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);

        using var response = await client.GetAsync(
            "/.well-known/openid-configuration",
            cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, responseBody);
        using var document = JsonDocument.Parse(responseBody);
        var root = document.RootElement;
        root.GetProperty("issuer").GetString().ShouldBe("https://localhost:7091/");
        root.GetProperty("authorization_endpoint").GetString().ShouldBe("https://localhost/connect/authorize");
        root.GetProperty("pushed_authorization_request_endpoint").GetString().ShouldBe("https://localhost/connect/par");
        root.GetProperty("token_endpoint").GetString().ShouldBe("https://localhost/connect/token");
        root.GetProperty("userinfo_endpoint").GetString().ShouldBe("https://localhost/connect/userinfo");
        root.GetProperty("introspection_endpoint").GetString().ShouldBe("https://localhost/connect/introspect");
        root.GetProperty("revocation_endpoint").GetString().ShouldBe("https://localhost/connect/revoke");
        root.GetProperty("end_session_endpoint").GetString().ShouldBe("https://localhost/connect/logout");
    }
}
