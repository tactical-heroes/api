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
                await applicationManager.CreateAsync(
                    descriptor,
                    cancellationToken);

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
            ClientType = OpenIddictConstants.ClientTypes.Public,
            DisplayName = client.DisplayName
        };

        descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Token);
        descriptor.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.Password);
        descriptor.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.RefreshToken);
        descriptor.Permissions.Add(OpenIddictConstants.Permissions.Scopes.Email);
        descriptor.Permissions.Add(OpenIddictConstants.Permissions.Scopes.Profile);
        descriptor.Permissions.Add(OpenIddictConstants.Permissions.Scopes.Roles);

        return descriptor;
    }
}
