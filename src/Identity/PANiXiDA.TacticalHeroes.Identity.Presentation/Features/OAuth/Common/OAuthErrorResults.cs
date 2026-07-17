using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

using OpenIddict.Server.AspNetCore;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Common;

internal static class OAuthErrorResults
{
    internal static ForbidHttpResult InvalidGrant(string description)
    {
        return Forbid(error: OpenIddictConstants.Errors.InvalidGrant, description: description);
    }

    internal static ForbidHttpResult UnsupportedGrantType(string description)
    {
        return Forbid(error: OpenIddictConstants.Errors.UnsupportedGrantType, description: description);
    }

    internal static ChallengeHttpResult InvalidToken(string description)
    {
        return TypedResults.Challenge(
            properties: CreateProperties(error: OpenIddictConstants.Errors.InvalidToken, description: description),
            authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
    }

    internal static ForbidHttpResult LoginRequired(string description)
    {
        return Forbid(error: OpenIddictConstants.Errors.LoginRequired, description: description);
    }

    private static ForbidHttpResult Forbid(string error, string description)
    {
        return TypedResults.Forbid(
            properties: CreateProperties(error: error, description: description),
            authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
    }

    private static AuthenticationProperties CreateProperties(
        string error,
        string description)
    {
        return new AuthenticationProperties(
            items: new Dictionary<string, string?>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = error,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = description
            });
    }
}
