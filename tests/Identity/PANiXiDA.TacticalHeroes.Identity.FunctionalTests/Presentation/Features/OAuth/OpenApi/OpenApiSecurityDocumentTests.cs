using System.Text.Json;

using Microsoft.Extensions.Hosting;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth.OpenApi;

public sealed class OpenApiSecurityDocumentTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Theory(DisplayName = "GET OpenAPI document should describe Bearer authentication when endpoint requires token")]
    [InlineData("identity-v1", "/api/v1/users", "get")]
    [InlineData("identity-v1", "/connect/userinfo", "get")]
    [InlineData("identity-v1", "/connect/userinfo", "post")]
    [InlineData("compendium-v1", "/api/v1/factions", "post")]
    public async Task GetOpenApiDocument_Should_DescribeBearerAuthentication_When_EndpointRequiresToken(
        string documentName,
        string path,
        string method)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = Fixture.CreateClient(Environments.Development);

        using var response = await client.GetAsync(
            $"/openapi/{documentName}.json",
            cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, responseBody);
        using var document = JsonDocument.Parse(responseBody);
        var scheme = document.RootElement
            .GetProperty("components")
            .GetProperty("securitySchemes")
            .GetProperty("Bearer");
        scheme.GetProperty("type").GetString().ShouldBe("http");
        scheme.GetProperty("scheme").GetString().ShouldBe("bearer");

        var security = document.RootElement
            .GetProperty("paths")
            .GetProperty(path)
            .GetProperty(method)
            .GetProperty("security");
        security.EnumerateArray().ShouldContain(requirement =>
            requirement.GetProperty("Bearer").GetArrayLength() == 0);
    }

    [Theory(DisplayName = "GET Identity OpenAPI document should not require Bearer authentication when endpoint allows anonymous")]
    [InlineData("/connect/authorize", "get")]
    [InlineData("/connect/token", "post")]
    public async Task GetIdentityOpenApiDocument_Should_NotRequireBearerAuthentication_When_EndpointAllowsAnonymous(
        string path,
        string method)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = Fixture.CreateClient(Environments.Development);

        using var response = await client.GetAsync(
            "/openapi/identity-v1.json",
            cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, responseBody);
        using var document = JsonDocument.Parse(responseBody);
        document.RootElement.TryGetProperty("security", out _).ShouldBeFalse();

        var operation = document.RootElement
            .GetProperty("paths")
            .GetProperty(path)
            .GetProperty(method);
        operation.TryGetProperty("security", out _).ShouldBeFalse();
    }
}
