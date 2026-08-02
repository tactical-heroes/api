using System.Text.Json;

using Microsoft.Extensions.Hosting;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth.OpenApi;

public sealed class IdentityOpenApiDocumentTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "GET Identity OpenAPI document should include OAuth endpoints when requested")]
    public async Task GetIdentityOpenApiDocument_Should_IncludeOAuthEndpoints_When_Requested()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = Fixture.CreateClient(Environments.Development);

        using var response = await client.GetAsync(
            "/openapi/identity.json",
            cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, responseBody);
        using var document = JsonDocument.Parse(responseBody);
        var paths = document.RootElement
            .GetProperty("paths")
            .EnumerateObject()
            .Select(path => path.Name)
            .ToArray();

        paths.ShouldContain("/connect/authorize");
        paths.ShouldContain("/connect/introspect");
        paths.ShouldContain("/connect/logout");
        paths.ShouldContain("/connect/par");
        paths.ShouldContain("/connect/revoke");
        paths.ShouldContain("/connect/token");
        paths.ShouldContain("/connect/userinfo");
    }
}
