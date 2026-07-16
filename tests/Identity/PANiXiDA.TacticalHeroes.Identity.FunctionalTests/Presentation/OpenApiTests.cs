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
        var expectedOperations = new (string Path, string Method, string Tag)[]
        {
            ("/api/v1/accounts/register", "post", "Accounts"),
            ("/api/v1/accounts/confirm", "post", "Accounts"),
            ("/api/v1/accounts/change-password", "post", "Accounts"),
            ("/api/v1/accounts/resend-confirmation-email", "post", "Accounts"),
            ("/api/v1/accounts/forgot-password", "post", "Accounts"),
            ("/api/v1/accounts/reset-password", "post", "Accounts"),
            ("/api/v1/accounts", "post", "Accounts"),
            ("/api/v1/accounts", "get", "Accounts"),
            ("/api/v1/accounts/{id}", "get", "Accounts"),
            ("/api/v1/accounts/{id}", "put", "Accounts"),
            ("/api/v1/accounts/{id}", "delete", "Accounts"),
            ("/api/v1/accounts/statuses", "get", "Accounts"),
            ("/api/v1/accounts/{id}/block", "post", "Accounts"),
            ("/api/v1/accounts/{id}/unblock", "post", "Accounts"),
            ("/api/v1/auth/login", "post", "Auth"),
            ("/api/v1/roles", "post", "Roles"),
            ("/api/v1/roles", "get", "Roles"),
            ("/api/v1/roles/{id}", "get", "Roles"),
            ("/api/v1/roles/{id}", "put", "Roles"),
            ("/api/v1/roles/{id}", "delete", "Roles"),
            ("/connect/par", "post", "OAuth"),
            ("/connect/authorize", "get", "OAuth"),
            ("/connect/token", "post", "OAuth"),
            ("/connect/userinfo", "get", "OAuth"),
            ("/connect/userinfo", "post", "OAuth"),
            ("/connect/logout", "get", "OAuth"),
            ("/connect/logout", "post", "OAuth"),
            ("/connect/introspect", "post", "OAuth"),
            ("/connect/revoke", "post", "OAuth")
        };

        foreach (var (path, method, tag) in expectedOperations)
        {
            var operation = GetOperation(paths, path, method);
            operation
                .GetProperty("tags")
                .EnumerateArray()
                .Select(element => element.GetString())
                .ShouldContain(tag);

            operation
                .GetProperty("responses")
                .TryGetProperty(StatusCodes.Status501NotImplemented.ToString(), out _)
                .ShouldBeTrue($"{method.ToUpperInvariant()} {path} should document the presentation-only 501 response");
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
