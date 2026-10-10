using System.Text.Json;
using System.Text.Json.Nodes;

using Microsoft.Extensions.Hosting;

using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth;

using static PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Users.UsersApiConstants;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Users;

public sealed class UserRequestValidationTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Theory(DisplayName = "User requests should return domain validation errors when status is invalid")]
    [InlineData(false, "", "User status is required.")]
    [InlineData(true, "", "User status is required.")]
    [InlineData(false, "   ", "User status is required.")]
    [InlineData(true, "   ", "User status is required.")]
    [InlineData(false, "Deleted", "User status 'Deleted' is invalid.")]
    [InlineData(true, "Deleted", "User status 'Deleted' is invalid.")]
    public async Task SendUserRequest_Should_ReturnValidationProblem_When_StatusIsInvalid(
        bool update,
        string status,
        string expectedMessage)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var payload = JsonSerializer.SerializeToNode(
            UserApiTestClient.CreateDefaultRequest() with { Status = status }, JsonOptions)!.AsObject();

        using var response = await SendUserRequestAsync(update, payload, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest, responseBody);
        using var problem = JsonDocument.Parse(responseBody);
        var errors = problem.RootElement.GetProperty("errors").GetProperty("status")
            .EnumerateArray().Select(error => error.GetString()).ToArray();
        errors.ShouldBe([expectedMessage]);
    }

    [Theory(DisplayName = "User requests should reject missing or null status through the JSON contract")]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task SendUserRequest_Should_ReturnBadRequest_When_StatusIsMissingOrNull(bool update, bool omitStatus)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var payload = JsonSerializer.SerializeToNode(UserApiTestClient.CreateDefaultRequest(), JsonOptions)!.AsObject();
        if (omitStatus)
        {
            payload.Remove("status");
        }
        else
        {
            payload["status"] = null;
        }

        using var response = await SendUserRequestAsync(update, payload, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest, responseBody);
    }

    [Theory(DisplayName = "User OpenAPI schemas should preserve required fields without validation attributes")]
    [InlineData("CreateUserRequest", true)]
    [InlineData("UpdateUserRequest", false)]
    public async Task GetOpenApiDocument_Should_PreserveUserJsonContract_When_Requested(
        string schemaName,
        bool includePassword)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = Fixture.CreateClient(Environments.Development);

        using var response = await client.GetAsync("/openapi/identity-v1.json", cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, responseBody);
        using var document = JsonDocument.Parse(responseBody);
        var schema = document.RootElement.GetProperty("components").GetProperty("schemas").GetProperty(schemaName);
        var requiredProperties = schema.GetProperty("required").EnumerateArray()
            .Select(property => property.GetString()!).ToArray();
        string[] expectedProperties = includePassword
            ? ["email", "userName", "password", "isConfirmed", "claims", "status"]
            : ["email", "userName", "isConfirmed", "claims", "status"];
        requiredProperties.ShouldBe(expectedProperties, ignoreOrder: true);
        var status = schema.GetProperty("properties").GetProperty("status");
        status.GetProperty("type").GetString().ShouldBe("string");
        status.TryGetProperty("maxLength", out _).ShouldBeFalse();
    }

    private async Task<HttpResponseMessage> SendUserRequestAsync(
        bool update,
        JsonObject payload,
        CancellationToken cancellationToken)
    {
        var path = UsersPath;
        if (update)
        {
            var client = new UserApiTestClient(Fixture);
            var user = await client.CreateAsync(cancellationToken);
            path = $"{path}/{user.Id}";
            payload.Remove("password");
        }

        using var request = new HttpRequestMessage(update ? HttpMethod.Put : HttpMethod.Post, path)
        {
            Content = JsonContent.Create(payload, options: JsonOptions)
        };

        return await OAuthServiceAccessTokenTestHelper.SendAsync(Fixture, request, cancellationToken);
    }
}
