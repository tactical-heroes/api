using System.Net.Http.Headers;
using System.Text.Json;

using Microsoft.Extensions.Hosting;

using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Roles.GetSelectOptions;

public sealed class GetRoleSelectOptionsEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "GET role select options should describe an array contract when document is requested")]
    public async Task GetRoleSelectOptions_Should_DescribeArrayContract_When_DocumentIsRequested()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = Fixture.CreateClient(Environments.Development);

        using var response = await client.GetAsync("/openapi/identity-v1.json", cancellationToken);
        var document = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var operation = document.GetProperty("paths").GetProperty("/api/v1/roles/select-options").GetProperty("get");
        operation.GetProperty("operationId").GetString().ShouldBe("GetRoleSelectOptions");
        var array = operation.GetProperty("responses")
            .GetProperty("200")
            .GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("schema");
        array.GetProperty("type").GetString().ShouldBe("array");
        array.GetProperty("items").GetProperty("$ref").GetString()
            .ShouldBe("#/components/schemas/RoleSelectOptionResponse");
        var schema = document.GetProperty("components").GetProperty("schemas").GetProperty("RoleSelectOptionResponse");
        schema.GetProperty("required").EnumerateArray().Select(property => property.GetString())
            .ShouldBe(["id", "name", "isDefault"]);
    }

    [Fact(DisplayName = "GET role select options should return sorted compact options when roles exist")]
    public async Task GetRoleSelectOptions_Should_ReturnSortedCompactOptions_When_RolesExist()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/roles/select-options");

        using var response = await OAuthServiceAccessTokenTestHelper.SendAsync(Fixture, request, cancellationToken);
        var options = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        options.EnumerateArray().Select(option => option.GetProperty("name").GetString())
            .ShouldBe(["Developer", "Moderator", "Player"]);
        options.EnumerateArray().Select(option => option.GetProperty("isDefault").GetBoolean())
            .ShouldBe([false, false, true]);
        foreach (var option in options.EnumerateArray())
        {
            option.GetProperty("id").GetGuid().ShouldNotBe(Guid.Empty);
            option.EnumerateObject().Select(property => property.Name).ShouldBe(["id", "name", "isDefault"]);
        }
    }

    [Theory(DisplayName = "GET role select options should require read permission when authenticated")]
    [InlineData("unrelated", HttpStatusCode.Forbidden)]
    [InlineData("identity.users.create", HttpStatusCode.Forbidden)]
    [InlineData("identity.users.update", HttpStatusCode.Forbidden)]
    [InlineData("identity.roles.read", HttpStatusCode.OK)]
    public async Task GetRoleSelectOptions_Should_RequireReadPermission_When_Authenticated(
        string permission, HttpStatusCode expectedStatus)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/roles/select-options");
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await OAuthServiceAccessTokenTestHelper.IssueAccessTokenAsync(Fixture, cancellationToken, [permission]));

        using var response = await Client.SendAsync(request, cancellationToken);

        response.StatusCode.ShouldBe(expectedStatus, await response.Content.ReadAsStringAsync(cancellationToken));
    }

    [Fact(DisplayName = "GET role select options should return unauthorized when token is missing")]
    public async Task GetRoleSelectOptions_Should_ReturnUnauthorized_When_TokenIsMissing()
    {
        using var response = await Client.GetAsync(
            "/api/v1/roles/select-options", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
