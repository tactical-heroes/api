namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options;

public sealed class IdentityProviderClientOptions
{
    public string ClientId { get; init; } = "tactical-heroes-web";

    public string DisplayName { get; init; } = "Tactical Heroes Web";

    public string ClientSecret { get; init; } = string.Empty;

    public string ClientType { get; init; } = OpenIddictConstants.ClientTypes.Public;

    public List<string> GrantTypes { get; init; } =
    [
        OpenIddictConstants.GrantTypes.AuthorizationCode,
        OpenIddictConstants.GrantTypes.RefreshToken
    ];

    public List<string> RedirectUris { get; init; } = [];

    public List<string> PostLogoutRedirectUris { get; init; } = [];

    public List<string> Scopes { get; init; } =
    [
        OpenIddictConstants.Scopes.OpenId,
        OpenIddictConstants.Scopes.OfflineAccess,
        OpenIddictConstants.Scopes.Profile,
        OpenIddictConstants.Scopes.Email,
        OpenIddictConstants.Scopes.Roles
    ];
}
