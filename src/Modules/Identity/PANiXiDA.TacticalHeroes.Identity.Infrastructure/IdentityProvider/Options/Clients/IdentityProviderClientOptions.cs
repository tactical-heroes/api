namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options.Clients;

public sealed class IdentityProviderClientOptions
{
    public string ClientId { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string ClientSecret { get; init; } = string.Empty;

    public string ClientType { get; init; } = OpenIddictConstants.ClientTypes.Public;

    public List<string> GrantTypes { get; init; } = [];

    public List<string> RedirectUris { get; init; } = [];

    public List<string> PostLogoutRedirectUris { get; init; } = [];

    public List<string> Scopes { get; init; } = [];
}
