using System.Security.Claims;

using Microsoft.AspNetCore.Identity;

using Riok.Mapperly.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Login;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Login;

[Mapper]
internal static partial class LoginMapper
{
    [MapperIgnoreSource(nameof(LoginRequest.ReturnUrl))]
    internal static partial LoginCommand ToCommand(LoginRequest request);

    internal static ClaimsPrincipal ToClaimsPrincipal(AuthenticatedAccountReadModel account)
    {
        var claims = new List<System.Security.Claims.Claim>
        {
            new(OpenIddictConstants.Claims.Subject, account.Id.ToString()),
            new(OpenIddictConstants.Claims.Name, account.UserName),
            new(OpenIddictConstants.Claims.Email, account.Email)
        };
        claims.AddRange(
            account.Claims.Where(claim =>
                claim.Type != OpenIddictConstants.Claims.Subject &&
                claim.Type != OpenIddictConstants.Claims.Name &&
                claim.Type != OpenIddictConstants.Claims.Email));

        var identity = new ClaimsIdentity(
            claims,
            IdentityConstants.ApplicationScheme,
            OpenIddictConstants.Claims.Name,
            OpenIddictConstants.Claims.Role);

        return new ClaimsPrincipal(identity);
    }
}
