using System.Security.Claims;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Common;

internal static class OAuthRequestScopes
{
    internal static IEnumerable<string> GetRequestedOrPrincipalScopes(
        OpenIddictRequest request,
        ClaimsPrincipal? principal)
    {
        var requestedScopes = request.GetScopes();

        return requestedScopes.Any()
            ? requestedScopes
            : principal?.GetScopes() ?? [];
    }
}
