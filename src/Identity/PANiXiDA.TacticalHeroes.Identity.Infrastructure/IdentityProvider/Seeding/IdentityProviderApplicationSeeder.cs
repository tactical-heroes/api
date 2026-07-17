using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Seeding;

internal sealed class IdentityProviderApplicationSeeder(
    IOpenIddictApplicationManager applicationManager,
    IOptions<IdentityProviderOptions> options)
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        foreach (var client in options.Value.Clients)
        {
            if (string.IsNullOrWhiteSpace(client.ClientId))
            {
                continue;
            }

            var descriptor = CreateDescriptor(client);
            var application = await applicationManager.FindByClientIdAsync(
                client.ClientId,
                cancellationToken);

            if (application is null)
            {
                await applicationManager.CreateAsync(descriptor, cancellationToken);
                continue;
            }

            await applicationManager.UpdateAsync(
                application,
                descriptor,
                cancellationToken);
        }
    }

    private static OpenIddictApplicationDescriptor CreateDescriptor(
        IdentityProviderClientOptions client)
    {
        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = client.ClientId,
            ClientSecret = string.IsNullOrWhiteSpace(client.ClientSecret)
                ? null
                : client.ClientSecret,
            ClientType = client.ClientType,
            DisplayName = client.DisplayName
        };

        descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Introspection);
        descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Token);
        descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Revocation);

        foreach (var grantType in client.GrantTypes)
        {
            AddGrantTypePermissions(descriptor, grantType);
        }

        if (client.PostLogoutRedirectUris.Count > 0)
        {
            descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.EndSession);
        }

        descriptor.AddScopePermissions([.. client.Scopes]);

        foreach (var redirectUri in client.RedirectUris)
        {
            descriptor.RedirectUris.Add(new Uri(redirectUri));
        }

        foreach (var postLogoutRedirectUri in client.PostLogoutRedirectUris)
        {
            descriptor.PostLogoutRedirectUris.Add(new Uri(postLogoutRedirectUri));
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
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Authorization);
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.PushedAuthorization);
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode);
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.Code);
                break;

            case OpenIddictConstants.GrantTypes.RefreshToken:
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.RefreshToken);
                break;

            case OpenIddictConstants.GrantTypes.ClientCredentials:
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.ClientCredentials);
                break;

            case OpenIddictConstants.GrantTypes.TokenExchange:
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.TokenExchange);
                break;

            default:
                throw new InvalidOperationException(
                    $"Unsupported OAuth grant type '{grantType}'.");
        }
    }
}
