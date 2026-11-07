using System.Text.Json;

using Microsoft.Extensions.Hosting;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth.OpenApi;

[Collection(FunctionalTestCollectionDefinition.Name)]
public sealed class ScalarOperationOrderingTests(FunctionalTestFixture fixture) : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "Scalar configuration should list creation operations when requested")]
    public async Task GetConfiguration_Should_ListCreationOperations_When_Requested()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = Fixture.CreateClient(Environments.Development);

        var scalar = await client.GetStringAsync("/scalar", cancellationToken);
        using var response = await client.GetAsync("/scalar/operation-order.js", cancellationToken);
        var configuration = await response.Content.ReadAsStringAsync(cancellationToken);

        scalar.ShouldContain("/scalar/operation-order.js");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.ShouldBe("text/javascript");
        string[] creationPaths =
        [
            "/api/v1/files", "/api/v1/files/personal", "/api/v1/files/upload-url",
            "/api/v1/files/personal/upload-url", "/api/v1/folders", "/api/v1/folders/personal",
            "/api/v1/users", "/api/v1/auth/register", "/api/v1/factions"
        ];
        foreach (var path in creationPaths)
        {
            configuration.ShouldContain(JsonSerializer.Serialize(path));
        }
        string[] actionPaths =
        [
            "/api/v1/files/{id}/complete-upload", "/api/v1/users/{id}/block",
            "/api/v1/users/{id}/unblock", "/api/v1/auth/login"
        ];
        foreach (var path in actionPaths)
        {
            configuration.ShouldNotContain(JsonSerializer.Serialize(path));
        }
        using var document = await client.GetAsync("/openapi/file-manager-v1.json", cancellationToken);
        document.StatusCode.ShouldBe(HttpStatusCode.OK);
        var openApi = await document.Content.ReadAsStringAsync(cancellationToken);
        using var parsed = JsonDocument.Parse(openApi);
        parsed.RootElement.GetProperty("paths").TryGetProperty("/scalar/operation-order.js", out _).ShouldBeFalse();
    }
}
