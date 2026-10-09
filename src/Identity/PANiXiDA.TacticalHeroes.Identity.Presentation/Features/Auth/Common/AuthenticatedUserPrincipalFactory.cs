using System.Security.Claims;

using Microsoft.AspNetCore.Identity;

using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Login;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Common;

internal static class AuthenticatedUserPrincipalFactory
{
    internal static ClaimsPrincipal Create(AuthenticatedUserReadModel user)
    {
        var claims = new List<Claim>
        {
            new(type: OpenIddictConstants.Claims.Subject, value: user.Id.ToString()),
            new(type: OpenIddictConstants.Claims.Name, value: user.UserName),
            new(type: OpenIddictConstants.Claims.Email, value: user.Email)
        };
        claims.AddRange(
            user.Claims.Where(claim =>
                claim.Type != OpenIddictConstants.Claims.Subject &&
                claim.Type != OpenIddictConstants.Claims.Name &&
                claim.Type != OpenIddictConstants.Claims.Email));

        var identity = new ClaimsIdentity(
            claims: claims,
            authenticationType: IdentityConstants.ApplicationScheme,
            nameType: OpenIddictConstants.Claims.Name,
            roleType: OpenIddictConstants.Claims.Role);

        return new ClaimsPrincipal(identity: identity);
    }
}
