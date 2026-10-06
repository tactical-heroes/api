using System.Text.Json;

using Microsoft.Extensions.Hosting;

using OpenIddict.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth.OpenApi;

public sealed class IdentityOpenApiDocumentTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "GET Identity OpenAPI document should require only mandatory PAR fields when requested")]
    public async Task GetIdentityOpenApiDocument_Should_RequireOnlyMandatoryParFields_When_Requested()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = Fixture.CreateClient(Environments.Development);

        using var response = await client.GetAsync("/openapi/identity-v1.json", cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, responseBody);
        using var document = JsonDocument.Parse(responseBody);
        var schema = document.RootElement.GetProperty("components").GetProperty("schemas").GetProperty("ParRequest");
        var requiredProperties = schema.GetProperty("required").EnumerateArray()
            .Select(property => property.GetString()!)
            .ToArray();

        requiredProperties.ShouldBe(
            [
                OpenIddictConstants.Parameters.ResponseType,
                OpenIddictConstants.Parameters.ClientId,
                OpenIddictConstants.Parameters.RedirectUri,
                OpenIddictConstants.Parameters.CodeChallenge,
                OpenIddictConstants.Parameters.CodeChallengeMethod
            ],
            ignoreOrder: true);
        var scopeTypes = schema.GetProperty("properties").GetProperty(OpenIddictConstants.Parameters.Scope)
            .GetProperty("type").EnumerateArray().Select(type => type.GetString()).ToArray();
        scopeTypes.ShouldContain("null");
    }

    [Theory(DisplayName = "GET Compendium OpenAPI document should make ranged attack fields optional when requested")]
    [InlineData("CreateUnitRequest")]
    [InlineData("UpdateUnitRequest")]
    public async Task GetCompendiumOpenApiDocument_Should_MakeRangedAttackFieldsOptional_When_Requested(
        string schemaName)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = Fixture.CreateClient(Environments.Development);

        using var response = await client.GetAsync("/openapi/compendium-v1.json", cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, responseBody);
        using var document = JsonDocument.Parse(responseBody);
        var schema = document.RootElement.GetProperty("components").GetProperty("schemas").GetProperty(schemaName);
        var requiredProperties = schema.GetProperty("required").EnumerateArray()
            .Select(property => property.GetString()!)
            .ToArray();

        requiredProperties.ShouldBe(
            [
                "name", "description", "attack", "defense", "health", "minimumDamage", "maximumDamage",
                "initiative", "speed", "morale", "luck", "factionId"
            ],
            ignoreOrder: true);
        foreach (var propertyName in new[] { "shots", "rangedAttackRange" })
        {
            var types = schema.GetProperty("properties").GetProperty(propertyName)
                .GetProperty("type").EnumerateArray().Select(type => type.GetString()).ToArray();
            types.ShouldContain("null");
        }
    }

    [Fact(DisplayName = "GET Identity OpenAPI document should include OAuth endpoints when requested")]
    public async Task GetIdentityOpenApiDocument_Should_IncludeOAuthEndpoints_When_Requested()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = Fixture.CreateClient(Environments.Development);

        using var response = await client.GetAsync(
            "/openapi/identity-v1.json",
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

        var logoutQueryParameters = document.RootElement
            .GetProperty("paths")
            .GetProperty("/connect/logout")
            .GetProperty("get")
            .GetProperty("parameters")
            .EnumerateArray()
            .Where(parameter => parameter.GetProperty("in").GetString() == "query")
            .Select(parameter => parameter.GetProperty("name").GetString()!)
            .ToArray();

        logoutQueryParameters.ShouldBe(
            [
                OpenIddictConstants.Parameters.ClientId,
                OpenIddictConstants.Parameters.IdTokenHint,
                OpenIddictConstants.Parameters.PostLogoutRedirectUri,
                OpenIddictConstants.Parameters.State,
                OpenIddictConstants.Parameters.UiLocales
            ],
            ignoreOrder: true);
    }
}
