using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options.Clients;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Seeding;

internal sealed class IdentityProviderApplicationSeeder(
    IOpenIddictApplicationManager applicationManager,
    IOptions<IdentityProviderOptions> options)
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        foreach (var client in options.Value.Clients)
        {
            if (string.IsNullOrWhiteSpace(value: client.ClientId))
            {
                continue;
            }

            var descriptor = CreateDescriptor(client: client);
            var application = await applicationManager.FindByClientIdAsync(
                identifier: client.ClientId,
                cancellationToken: cancellationToken);

            if (application is null)
            {
                await applicationManager.CreateAsync(descriptor: descriptor, cancellationToken: cancellationToken);
                continue;
            }

            await applicationManager.UpdateAsync(
                application: application,
                descriptor: descriptor,
                cancellationToken: cancellationToken);
        }
    }

    private static OpenIddictApplicationDescriptor CreateDescriptor(
        IdentityProviderClientOptions client)
    {
        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = client.ClientId,
            ClientSecret = string.IsNullOrWhiteSpace(value: client.ClientSecret)
                ? null
                : client.ClientSecret,
            ClientType = client.ClientType,
            DisplayName = client.DisplayName
        };

        descriptor.Permissions.Add(item: OpenIddictConstants.Permissions.Endpoints.Introspection);
        descriptor.Permissions.Add(item: OpenIddictConstants.Permissions.Endpoints.Token);
        descriptor.Permissions.Add(item: OpenIddictConstants.Permissions.Endpoints.Revocation);

        foreach (var grantType in client.GrantTypes)
        {
            AddGrantTypePermissions(descriptor: descriptor, grantType: grantType);
        }

        if (client.PostLogoutRedirectUris.Count > 0)
        {
            descriptor.Permissions.Add(item: OpenIddictConstants.Permissions.Endpoints.EndSession);
        }

        descriptor.AddScopePermissions(scopes: [.. client.Scopes]);

        foreach (var redirectUri in client.RedirectUris)
        {
            descriptor.RedirectUris.Add(item: new Uri(uriString: redirectUri));
        }

        foreach (var postLogoutRedirectUri in client.PostLogoutRedirectUris)
        {
            descriptor.PostLogoutRedirectUris.Add(item: new Uri(uriString: postLogoutRedirectUri));
        }

        return descriptor;
    }

    private static void AddGrantTypePermissions(
        OpenIddictApplicationDescriptor descriptor,
        string grantType)
    {
        switch (grantType)
        {
            case OpenIddictConstants.GrantTypes.AuthorizationCode:
                descriptor.Permissions.Add(item: OpenIddictConstants.Permissions.Endpoints.Authorization);
                descriptor.Permissions.Add(item: OpenIddictConstants.Permissions.Endpoints.PushedAuthorization);
                descriptor.Permissions.Add(item: OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode);
                descriptor.Permissions.Add(item: OpenIddictConstants.Permissions.ResponseTypes.Code);
                break;

            case OpenIddictConstants.GrantTypes.RefreshToken:
                descriptor.Permissions.Add(item: OpenIddictConstants.Permissions.GrantTypes.RefreshToken);
                break;

            case OpenIddictConstants.GrantTypes.ClientCredentials:
                descriptor.Permissions.Add(item: OpenIddictConstants.Permissions.GrantTypes.ClientCredentials);
                break;

            case OpenIddictConstants.GrantTypes.TokenExchange:
                descriptor.Permissions.Add(item: OpenIddictConstants.Permissions.GrantTypes.TokenExchange);
                break;

            default:
                throw new InvalidOperationException(
                    message: $"Unsupported OAuth grant type '{grantType}'.");
        }
    }
}
