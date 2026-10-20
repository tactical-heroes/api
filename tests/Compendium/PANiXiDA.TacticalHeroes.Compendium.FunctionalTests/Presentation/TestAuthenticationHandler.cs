using System.Security.Claims;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;

namespace PANiXiDA.TacticalHeroes.Compendium.FunctionalTests.Presentation;

internal sealed class TestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(
        options,
        logger,
        encoder)
{
    internal const string SchemeName = "CompendiumTests";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var permissions = Request.Headers.TryGetValue("X-Test-Permissions", out var values)
            ? values.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries)
            : Permissions.All.Select(permission => permission.Code).ToArray();
        Claim[] claims =
        [
            new(ClaimTypes.NameIdentifier, Guid.CreateVersion7().ToString()),
            new(ClaimTypes.Name, "compendium-tests"),
            .. permissions.Select(permission => new Claim("permission", permission))
        ];
        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
