using System.Net.Http.Headers;
using System.Text.Json;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using OpenIddict.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth;

internal static class OAuthServiceAccessTokenTestHelper
{
    private const string TokenPath = "/connect/token";

    internal static async Task<HttpResponseMessage> SendAsync(
        FunctionalTestFixture fixture,
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await IssueAccessTokenAsync(fixture, cancellationToken));

        return await fixture.Client.SendAsync(request, cancellationToken);
    }

    private static async Task<string> IssueAccessTokenAsync(
        FunctionalTestFixture fixture,
        CancellationToken cancellationToken)
    {
        using var scope = fixture.Services.CreateScope();
        var options = scope.ServiceProvider
            .GetRequiredService<IOptions<IdentityProviderOptions>>()
            .Value;
        var client = options.Clients.Single(item =>
            string.Equals(
                item.ClientType,
                OpenIddictConstants.ClientTypes.Confidential,
                StringComparison.Ordinal) &&
            item.GrantTypes.Contains(
                OpenIddictConstants.GrantTypes.ClientCredentials,
                StringComparer.Ordinal));
        var parameters = new Dictionary<string, string>
        {
            [OpenIddictConstants.Parameters.GrantType] = OpenIddictConstants.GrantTypes.ClientCredentials,
            [OpenIddictConstants.Parameters.ClientId] = client.ClientId,
            [OpenIddictConstants.Parameters.ClientSecret] = client.ClientSecret
        };

        if (client.Scopes.Count > 0)
        {
            parameters[OpenIddictConstants.Parameters.Scope] = string.Join(' ', client.Scopes);
        }

        using var response = await fixture.Client.PostAsync(
            TokenPath,
            new FormUrlEncodedContent(parameters),
            cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, responseBody);

        using var document = JsonDocument.Parse(responseBody);

        return document.RootElement.GetProperty("access_token").GetString()
            ?? throw new InvalidOperationException("Access token was not returned.");
    }
}
