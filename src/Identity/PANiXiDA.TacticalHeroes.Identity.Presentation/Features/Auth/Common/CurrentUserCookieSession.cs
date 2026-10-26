using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Application.Auth.GetAuthenticatedUser;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Common;

internal sealed class CurrentUserCookieSession(
    ICurrentUser currentUser,
    IMediator mediator,
    IOptions<IdentityOptions> identityOptions)
{
    internal async Task<Snapshot?> CaptureAsync(HttpContext context, Guid? targetUserId = null)
    {
        if (currentUser.UserId is not { } userId || (targetUserId is { } targetId && targetId != userId))
        {
            return null;
        }

        var cookie = await context.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        if (!cookie.Succeeded
            || !Guid.TryParse(cookie.Principal?.GetClaim(OpenIddictConstants.Claims.Subject), out var cookieUserId)
            || cookieUserId != userId)
        {
            return null;
        }

        var securityStamp = cookie.Principal.GetClaim(identityOptions.Value.ClaimsIdentity.SecurityStampClaimType);
        if (string.IsNullOrEmpty(securityStamp))
        {
            return null;
        }

        return new Snapshot(UserId: userId, SecurityStamp: securityStamp, Properties: cookie.Properties);
    }

    internal async Task RenewAsync(HttpContext context, Snapshot? session, CancellationToken cancellationToken)
    {
        var authorizationId = HttpCurrentUserSessionRefresh.GetAuthorizationId(context);
        if (session is null || currentUser.UserId != session.UserId || authorizationId is null)
        {
            return;
        }

        var user = await mediator.QueryAsync(
            new GetAuthenticatedUserQuery(
                UserId: session.UserId,
                SecurityStamp: session.SecurityStamp,
                AuthorizationId: authorizationId),
            cancellationToken);
        if (user.IsFailure || user.Value.Id != session.UserId)
        {
            return;
        }

        await context.SignInAsync(
            scheme: IdentityConstants.ApplicationScheme,
            principal: AuthenticatedUserPrincipalFactory.Create(user.Value),
            properties: session.Properties);
    }

    internal sealed record Snapshot(
        Guid UserId,
        string SecurityStamp,
        AuthenticationProperties Properties);
}
