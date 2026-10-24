using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Application.Auth.GetAuthenticatedUser;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Common;

internal sealed class CurrentUserCookieSession(ICurrentUser currentUser, IMediator mediator)
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

        return new Snapshot(userId, cookie.Properties);
    }

    internal Task RenewIfRequiredAsync(HttpContext context, Snapshot? session, CancellationToken cancellationToken)
    {
        return HttpCurrentUserSessionRefresh.IsRequired(context)
            ? RenewAsync(context: context, session: session, cancellationToken: cancellationToken)
            : Task.CompletedTask;
    }

    internal async Task RenewAsync(HttpContext context, Snapshot? session, CancellationToken cancellationToken)
    {
        if (session is null || currentUser.UserId != session.UserId)
        {
            return;
        }

        var user = await mediator.QueryAsync(new GetAuthenticatedUserQuery(session.UserId), cancellationToken);
        if (user.IsFailure || user.Value.Id != session.UserId)
        {
            return;
        }

        await context.SignInAsync(
            scheme: IdentityConstants.ApplicationScheme,
            principal: AuthenticatedUserPrincipalFactory.Create(user.Value),
            properties: session.Properties);
    }

    internal sealed record Snapshot(Guid UserId, AuthenticationProperties Properties);
}
