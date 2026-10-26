using System.Net.Http.Headers;
using System.Text.Json;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using OpenIddict.Abstractions;
using OpenIddict.Validation;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth.Introspect;

[Collection(FunctionalTestCollectionDefinition.Name)]
public sealed class ApiTokenValidationTests(FunctionalTestFixture fixture) : FunctionalTestBase(fixture)
{
    [Theory(DisplayName = "Protected API should reject invalid tokens when token validation fails")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetUsers_Should_RejectToken_When_TokenValidationFails(bool wrongAudience)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await OAuthAuthorizationRequestTestHelper.CreateConfirmedUserAsync(
            fixture: Fixture,
            email: "validation@example.test",
            userName: "validation-hero",
            password: "StrongPassword1!",
            claims: null,
            cancellationToken: cancellationToken);
        using var oauthClient = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        var tokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            oauthClient, "validation@example.test", "StrongPassword1!", cancellationToken);
        await using var testFactory = Fixture.CreateFactory();
        await using var factory = testFactory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => services.PostConfigure<OpenIddictValidationOptions>(options =>
            {
                if (wrongAudience)
                {
                    options.Audiences.Clear();
                    options.Audiences.Add("another-api");
                }
            })));
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/users");
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer", wrongAudience ? tokens.AccessToken : "invalid-reference-token");

        using var response = await client.SendAsync(request, cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        testFactory.IntrospectionRequestCount.ShouldBe(1);
    }

    [Fact(DisplayName = "Existing service client should retain token grants when client credentials flow is requested")]
    public async Task PostToken_Should_UseExistingServiceClient_When_ClientCredentialsFlowIsRequested()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var options = Fixture.Services.GetRequiredService<IOptions<OpenIddictValidationOptions>>().Value;
        await using var scope = Fixture.Services.CreateAsyncScope();
        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        options.ClientId.ShouldNotBeNull();
        options.ClientId.ShouldBe("tactical-heroes-service");
        (await manager.CountAsync(cancellationToken)).ShouldBe(2);
        var application = await manager.FindByClientIdAsync(options.ClientId, cancellationToken);
        application.ShouldNotBeNull();
        var permissions = await manager.GetPermissionsAsync(application, cancellationToken);
        permissions.ShouldContain(OpenIddictConstants.Permissions.Endpoints.Introspection);
        permissions.ShouldContain(OpenIddictConstants.Permissions.GrantTypes.ClientCredentials);
        permissions.ShouldContain(OpenIddictConstants.Permissions.GrantTypes.TokenExchange);
        using var client = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);

        using var content = new FormUrlEncodedContent(
            new Dictionary<string, string?>
            {
                [OpenIddictConstants.Parameters.GrantType] = OpenIddictConstants.GrantTypes.ClientCredentials,
                [OpenIddictConstants.Parameters.ClientId] = options.ClientId,
                [OpenIddictConstants.Parameters.ClientSecret] = options.ClientSecret
            });

        using var response = await client.PostAsync(
            "/connect/token",
            content,
            cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, body);
        using var document = JsonDocument.Parse(body);
        document.RootElement.GetProperty(OpenIddictConstants.Parameters.AccessToken).GetString()
            .ShouldNotBeNullOrWhiteSpace();
    }

    [Fact(DisplayName = "Introspection should return user claims when service client introspects web token")]
    public async Task PostIntrospect_Should_ReturnUserClaims_When_ServiceClientIntrospectsWebToken()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var options = Fixture.Services.GetRequiredService<IOptions<OpenIddictValidationOptions>>().Value;
        var user = await OAuthAuthorizationRequestTestHelper.CreateConfirmedUserAsync(
            fixture: Fixture,
            email: "service-introspection@example.test",
            userName: "service-introspection-hero",
            password: "StrongPassword1!",
            claims: null,
            cancellationToken: cancellationToken);
        using var client = OAuthAuthorizationRequestTestHelper.CreateOAuthClient(Fixture);
        var tokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            client, "service-introspection@example.test", "StrongPassword1!", cancellationToken);

        using var content = new FormUrlEncodedContent(
            new Dictionary<string, string?>
            {
                [OpenIddictConstants.Parameters.ClientId] = options.ClientId,
                [OpenIddictConstants.Parameters.ClientSecret] = options.ClientSecret,
                [OpenIddictConstants.Parameters.Token] = tokens.AccessToken
            });

        using var response = await client.PostAsync(
            "/connect/introspect",
            content,
            cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, body);
        using var document = JsonDocument.Parse(body);
        document.RootElement.GetProperty(OpenIddictConstants.Claims.Active).GetBoolean().ShouldBeTrue();
        document.RootElement.GetProperty(OpenIddictConstants.Claims.Audience).GetString().ShouldBe(options.ClientId);
        document.RootElement.GetProperty(OpenIddictConstants.Claims.ClientId).GetString()
            .ShouldBe(OAuthAuthorizationRequestTestHelper.ClientId);
        document.RootElement.GetProperty(OpenIddictConstants.Claims.Subject).GetString().ShouldBe(user.Id.ToString());
        document.RootElement.GetProperty(OpenIddictConstants.Claims.Name).GetString()
            .ShouldBe("service-introspection-hero");
        document.RootElement.GetProperty(OpenIddictConstants.Claims.Email).GetString()
            .ShouldBe("service-introspection@example.test");
    }
}
