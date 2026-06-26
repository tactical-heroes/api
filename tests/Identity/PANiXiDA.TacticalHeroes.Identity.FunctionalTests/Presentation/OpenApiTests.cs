using System.Text.Json;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation;

public sealed class OpenApiTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "OpenAPI should describe OpenIddict token endpoint")]
    public async Task OpenApi_Should_DescribeOpenIddictTokenEndpoint()
    {
        using var response = await Client.GetAsync(
            "/openapi/v1.json",
            TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        await using var contentStream = await response.Content.ReadAsStreamAsync(
            TestContext.Current.CancellationToken);
        using var document = await JsonDocument.ParseAsync(
            contentStream,
            cancellationToken: TestContext.Current.CancellationToken);

        var tokenOperation = document.RootElement
            .GetProperty("paths")
            .GetProperty("/connect/token")
            .GetProperty("post");

        tokenOperation.GetProperty("summary").GetString()
            .ShouldBe("Exchange identity token");

        var formContent = tokenOperation
            .GetProperty("requestBody")
            .GetProperty("content")
            .GetProperty("application/x-www-form-urlencoded");

        formContent
            .GetProperty("schema")
            .GetProperty("$ref")
            .GetString()
            .ShouldBe("#/components/schemas/OpenIddictTokenRequest");
    }
}
