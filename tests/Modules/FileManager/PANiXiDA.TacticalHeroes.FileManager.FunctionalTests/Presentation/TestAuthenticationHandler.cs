using System.Security.Claims;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using OpenIddict.Abstractions;

using CoreClaimTypes = PANiXiDA.Core.Application.Authentication.ClaimTypes;

namespace PANiXiDA.TacticalHeroes.FileManager.FunctionalTests.Presentation;

internal sealed class TestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    internal const string SchemeName = "FileManagerTests";
    internal const string UserIdHeader = "X-Test-UserId";
    internal const string PermissionsHeader = "X-Test-Permissions";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var subject = Request.Headers[UserIdHeader].ToString();
        if (string.IsNullOrEmpty(subject))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var permissions = Request.Headers[PermissionsHeader].ToString()
            .Split(',', StringSplitOptions.RemoveEmptyEntries);
        Claim[] claims =
        [
            new(OpenIddictConstants.Claims.Subject, subject),
            .. permissions.Select(permission => new Claim(CoreClaimTypes.Permission, permission))
        ];
        var identity = new ClaimsIdentity(claims, SchemeName);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
