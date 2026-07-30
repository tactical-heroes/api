using System.Security.Claims;

using OpenIddict.Server.AspNetCore;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Common;

internal static class OAuthAuthorizationPrincipalFactory
{
    internal static ClaimsPrincipal Create(
        ClaimsPrincipal source,
        IEnumerable<string> scopes,
        string audience)
    {
        return Create(claims: source.Claims, scopes: scopes, audience: audience);
    }

    internal static ClaimsPrincipal Create(
        IEnumerable<Claim> claims,
        IEnumerable<string> scopes,
        string audience)
    {
        var identity = new ClaimsIdentity(
            claims: claims,
            authenticationType: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            nameType: OpenIddictConstants.Claims.Name,
            roleType: OpenIddictConstants.Claims.Role);
        var principal = new ClaimsPrincipal(identity: identity);

        principal.SetScopes(
            scopes
                .Where(scope => !string.IsNullOrWhiteSpace(scope))
                .Distinct(StringComparer.Ordinal));

        if (!string.IsNullOrWhiteSpace(audience))
        {
            principal.SetAudiences(audience);
        }

        principal.SetDestinations(claim => GetDestinations(claim, principal));

        return principal;
    }

    private static IEnumerable<string> GetDestinations(
        Claim claim,
        ClaimsPrincipal principal)
    {
        return claim.Type switch
        {
            OpenIddictConstants.Claims.Subject =>
            [
                OpenIddictConstants.Destinations.AccessToken,
                OpenIddictConstants.Destinations.IdentityToken
            ],
            OpenIddictConstants.Claims.Name when principal.HasScope(OpenIddictConstants.Scopes.Profile) =>
            [
                OpenIddictConstants.Destinations.AccessToken,
                OpenIddictConstants.Destinations.IdentityToken
            ],
            OpenIddictConstants.Claims.Email when principal.HasScope(OpenIddictConstants.Scopes.Email) =>
            [
                OpenIddictConstants.Destinations.AccessToken,
                OpenIddictConstants.Destinations.IdentityToken
            ],
            OpenIddictConstants.Claims.Role when principal.HasScope(OpenIddictConstants.Scopes.Roles) =>
            [
                OpenIddictConstants.Destinations.AccessToken,
                OpenIddictConstants.Destinations.IdentityToken
            ],
            _ => [OpenIddictConstants.Destinations.AccessToken]
        };
    }
}
