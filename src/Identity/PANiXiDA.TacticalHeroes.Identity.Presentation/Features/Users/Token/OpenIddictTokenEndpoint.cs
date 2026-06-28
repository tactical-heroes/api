using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

using OpenIddict.Server.AspNetCore;

using PANiXiDA.TacticalHeroes.Identity.Application.Users;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetAuthenticated;

using System.Security.Claims;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Token;

internal static class OpenIddictTokenEndpoint
{
    public static RouteHandlerBuilder MapOpenIddictTokenEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/connect/token", HandleAsync)
            .AllowAnonymous()
            .WithTags("Users")
            .WithName("ExchangeIdentityToken")
            .WithSummary("Exchange identity token")
            .WithDescription("OAuth2 token endpoint for password and refresh_token grant types.")
            .Accepts<OpenIddictTokenRequest>("application/x-www-form-urlencoded")
            .Produces<OpenIddictTokenResponse>(StatusCodes.Status200OK)
            .Produces<OpenIddictTokenErrorResponse>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        IUserAuthenticationService identityAuthenticationService,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var request = httpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("OpenIddict server request was not found.");

        if (request.IsPasswordGrantType())
        {
            var userResult = await identityAuthenticationService.AuthenticateAsync(
                request.Username ?? string.Empty,
                request.Password ?? string.Empty,
                cancellationToken);

            if (userResult.IsFailure)
            {
                return InvalidGrant(userResult.Errors[0].Message);
            }

            return SignIn(CreatePrincipal(userResult.Value, GetRequestedScopes(request)));
        }

        if (request.IsRefreshTokenGrantType())
        {
            var authenticateResult = await httpContext.AuthenticateAsync(
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var userIdValue = authenticateResult.Principal?.GetClaim(OpenIddictConstants.Claims.Subject);

            if (!Guid.TryParse(userIdValue, out var userId))
            {
                return InvalidGrant("Refresh token is invalid.");
            }

            var userResult = await mediator.QueryAsync(
                new GetAuthenticatedUserQuery(userId),
                cancellationToken);

            if (userResult.IsFailure)
            {
                return InvalidGrant("Refresh token is invalid.");
            }

            var scopes = request.GetScopes().Any()
                ? request.GetScopes()
                : authenticateResult.Principal?.GetScopes() ?? [];

            return SignIn(CreatePrincipal(userResult.Value, scopes));
        }

        return InvalidGrant("Grant type is not supported.");
    }

    private static IResult SignIn(ClaimsPrincipal principal)
    {
        return TypedResults.SignIn(
            principal,
            authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private static ClaimsPrincipal CreatePrincipal(
        AuthenticatedUser user,
        IEnumerable<string> scopes)
    {
        var identity = new ClaimsIdentity(
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            OpenIddictConstants.Claims.Name,
            OpenIddictConstants.Claims.Role);

        identity.AddClaim(OpenIddictConstants.Claims.Subject, user.Id.ToString());
        identity.AddClaim(OpenIddictConstants.Claims.Email, user.Email);
        identity.AddClaim(OpenIddictConstants.Claims.EmailVerified, user.IsConfirmed.ToString().ToLowerInvariant());

        foreach (var role in user.Roles)
        {
            identity.AddClaim(OpenIddictConstants.Claims.Role, role);
        }

        foreach (var claim in user.Claims)
        {
            identity.AddClaim(claim.Type, claim.Value);
        }

        var principal = new ClaimsPrincipal(identity);
        principal.SetScopes(NormalizeScopes(scopes));
        principal.SetDestinations(GetDestinations);

        return principal;
    }

    private static IEnumerable<string> GetRequestedScopes(OpenIddictRequest request)
    {
        var requestedScopes = request.GetScopes();

        return requestedScopes.Any()
            ? requestedScopes
            :
            [
                OpenIddictConstants.Scopes.Email,
                OpenIddictConstants.Scopes.OfflineAccess,
                OpenIddictConstants.Scopes.Profile,
                OpenIddictConstants.Scopes.Roles
            ];
    }

    private static IEnumerable<string> NormalizeScopes(IEnumerable<string> scopes)
    {
        return scopes
            .Where(scope => !string.IsNullOrWhiteSpace(scope))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    private static IEnumerable<string> GetDestinations(Claim claim)
    {
        return claim.Type switch
        {
            OpenIddictConstants.Claims.Subject => [OpenIddictConstants.Destinations.AccessToken],
            OpenIddictConstants.Claims.Email => [OpenIddictConstants.Destinations.AccessToken],
            OpenIddictConstants.Claims.EmailVerified => [OpenIddictConstants.Destinations.AccessToken],
            OpenIddictConstants.Claims.Role => [OpenIddictConstants.Destinations.AccessToken],
            _ => [OpenIddictConstants.Destinations.AccessToken]
        };
    }

    private static IResult InvalidGrant(string description)
    {
        return TypedResults.Forbid(
            new AuthenticationProperties(
                new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                        OpenIddictConstants.Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = description
                }),
            [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
    }
}
