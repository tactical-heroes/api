using System.Net.Http.Headers;
using System.Text.Json;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using OpenIddict.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth.ExchangeToken;

public sealed class ExchangeTokenEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Theory(
        DisplayName = "POST OAuth token should exchange and refresh a persisted user authorization when refresh token is valid")]
    [InlineData(null)]
    [InlineData("custom_stamp")]
    public async Task PostToken_Should_IssueNewTokens_When_RefreshTokenIsValid(string? securityStampClaimType)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await OAuthAuthorizationRequestTestHelper.CreateConfirmedUserAsync(
            fixture: Fixture,
            email: "exchange@example.test",
            userName: "exchange-hero",
            password: "StrongPassword1!",
            claims: null,
            cancellationToken: cancellationToken);
        await using var baseFactory = new FunctionalTestWebApplicationFactory();
        await using var factory = baseFactory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            services.Configure<IdentityOptions>(options =>
                options.ClaimsIdentity.SecurityStampClaimType = securityStampClaimType
                    ?? options.ClaimsIdentity.SecurityStampClaimType)));
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });
        var tokens = await OAuthAuthorizationRequestTestHelper.IssueUserTokensAsync(
            client,
            "exchange@example.test",
            "StrongPassword1!",
            cancellationToken);

        using var content = new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                [OpenIddictConstants.Parameters.GrantType] = OpenIddictConstants.GrantTypes.RefreshToken,
                [OpenIddictConstants.Parameters.ClientId] = OAuthAuthorizationRequestTestHelper.ClientId,
                [OpenIddictConstants.Parameters.RefreshToken] = tokens.RefreshToken
            });

        using var response = await client.PostAsync(
            "/connect/token",
            content,
            cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, responseBody);
        using var document = JsonDocument.Parse(responseBody);
        var accessToken = document.RootElement.GetProperty("access_token").GetString();
        accessToken.ShouldNotBeNullOrWhiteSpace();
        document.RootElement.GetProperty("refresh_token").GetString().ShouldNotBeNullOrWhiteSpace();

        using var userInfoRequest = new HttpRequestMessage(HttpMethod.Get, "/connect/userinfo");
        userInfoRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        using var userInfoResponse = await client.SendAsync(userInfoRequest, cancellationToken);
        userInfoResponse.StatusCode.ShouldBe(
            HttpStatusCode.OK, await userInfoResponse.Content.ReadAsStringAsync(cancellationToken));
    }
}
