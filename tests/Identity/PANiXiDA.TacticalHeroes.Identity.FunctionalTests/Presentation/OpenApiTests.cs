using System.Text.Json;

using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation;

public sealed class OpenApiTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "OpenAPI should describe the SpaceApp-compatible identity HTTP contract")]
    public async Task OpenApi_Should_DescribeSpaceAppCompatibleIdentityHttpContract()
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

        var paths = document.RootElement.GetProperty("paths");
        var expectedOperations = new (string Path, string Method, string Tag, int SuccessStatusCode)[]
        {
            ("/api/v1/accounts/register", "post", "Accounts", StatusCodes.Status201Created),
            ("/api/v1/accounts/confirm", "post", "Accounts", StatusCodes.Status204NoContent),
            ("/api/v1/accounts/change-password", "post", "Accounts", StatusCodes.Status204NoContent),
            ("/api/v1/accounts/resend-confirmation-email", "post", "Accounts", StatusCodes.Status204NoContent),
            ("/api/v1/accounts/forgot-password", "post", "Accounts", StatusCodes.Status202Accepted),
            ("/api/v1/accounts/reset-password", "post", "Accounts", StatusCodes.Status204NoContent),
            ("/api/v1/accounts", "post", "Accounts", StatusCodes.Status201Created),
            ("/api/v1/accounts", "get", "Accounts", StatusCodes.Status200OK),
            ("/api/v1/accounts/{id}", "get", "Accounts", StatusCodes.Status200OK),
            ("/api/v1/accounts/{id}", "put", "Accounts", StatusCodes.Status204NoContent),
            ("/api/v1/accounts/{id}", "delete", "Accounts", StatusCodes.Status204NoContent),
            ("/api/v1/accounts/statuses", "get", "Accounts", StatusCodes.Status200OK),
            ("/api/v1/accounts/{id}/block", "post", "Accounts", StatusCodes.Status204NoContent),
            ("/api/v1/accounts/{id}/unblock", "post", "Accounts", StatusCodes.Status204NoContent),
            ("/api/v1/auth/login", "post", "Auth", StatusCodes.Status302Found),
            ("/api/v1/roles", "post", "Roles", StatusCodes.Status201Created),
            ("/api/v1/roles", "get", "Roles", StatusCodes.Status200OK),
            ("/api/v1/roles/{id}", "get", "Roles", StatusCodes.Status200OK),
            ("/api/v1/roles/{id}", "put", "Roles", StatusCodes.Status204NoContent),
            ("/api/v1/roles/{id}", "delete", "Roles", StatusCodes.Status204NoContent),
            ("/connect/par", "post", "OAuth", StatusCodes.Status201Created),
            ("/connect/authorize", "get", "OAuth", StatusCodes.Status302Found),
            ("/connect/token", "post", "OAuth", StatusCodes.Status200OK),
            ("/connect/userinfo", "get", "OAuth", StatusCodes.Status200OK),
            ("/connect/userinfo", "post", "OAuth", StatusCodes.Status200OK),
            ("/connect/logout", "get", "OAuth", StatusCodes.Status302Found),
            ("/connect/logout", "post", "OAuth", StatusCodes.Status302Found),
            ("/connect/introspect", "post", "OAuth", StatusCodes.Status200OK),
            ("/connect/revoke", "post", "OAuth", StatusCodes.Status200OK)
        };

        foreach (var (path, method, tag, successStatusCode) in expectedOperations)
        {
            var operation = GetOperation(paths, path, method);
            operation
                .GetProperty("tags")
                .EnumerateArray()
                .Select(element => element.GetString())
                .ShouldContain(tag);

            var responses = operation.GetProperty("responses");
            responses
                .TryGetProperty(successStatusCode.ToString(), out _)
                .ShouldBeTrue($"{method.ToUpperInvariant()} {path} should document the {successStatusCode} response");
            responses
                .TryGetProperty(StatusCodes.Status501NotImplemented.ToString(), out _)
                .ShouldBeFalse($"{method.ToUpperInvariant()} {path} should not expose a presentation stub response");
        }

        paths.EnumerateObject().ShouldNotContain(path =>
            path.Name.StartsWith("/api/v1/identity", StringComparison.Ordinal) ||
            path.Name.StartsWith("/api/v1/users", StringComparison.Ordinal) ||
            path.Name.StartsWith("/api/v1/account/", StringComparison.Ordinal));
    }

    private static JsonElement GetOperation(
        JsonElement paths,
        string expectedPath,
        string method)
    {
        var path = paths
            .EnumerateObject()
            .SingleOrDefault(candidate =>
                string.Equals(
                    candidate.Name.TrimEnd('/'),
                    expectedPath,
                    StringComparison.Ordinal));

        path.Value.ValueKind.ShouldNotBe(
            JsonValueKind.Undefined,
            $"OpenAPI path '{expectedPath}' was not found");

        return path.Value.GetProperty(method);
    }
}
