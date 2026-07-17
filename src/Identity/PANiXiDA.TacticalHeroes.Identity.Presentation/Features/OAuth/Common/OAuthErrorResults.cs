using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

using OpenIddict.Server.AspNetCore;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Common;

internal static class OAuthErrorResults
{
    internal static ForbidHttpResult InvalidGrant(string description)
    {
        return Forbid(OpenIddictConstants.Errors.InvalidGrant, description);
    }

    internal static ForbidHttpResult UnsupportedGrantType(string description)
    {
        return Forbid(OpenIddictConstants.Errors.UnsupportedGrantType, description);
    }

    internal static ChallengeHttpResult InvalidToken(string description)
    {
        return TypedResults.Challenge(
            CreateProperties(OpenIddictConstants.Errors.InvalidToken, description),
            [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
    }

    internal static ForbidHttpResult LoginRequired(string description)
    {
        return Forbid(OpenIddictConstants.Errors.LoginRequired, description);
    }

    private static ForbidHttpResult Forbid(string error, string description)
    {
        return TypedResults.Forbid(
            CreateProperties(error, description),
            [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
    }

    private static AuthenticationProperties CreateProperties(
        string error,
        string description)
    {
        return new AuthenticationProperties(
            new Dictionary<string, string?>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = error,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = description
            });
    }
}
