using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Common;

internal sealed class CurrentUserCookieSession(ICurrentUser currentUser)
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

    internal Task RenewAsync(HttpContext context, Snapshot? session, AuthenticatedUserReadModel user)
    {
        if (session is null || session.UserId != user.Id || currentUser.UserId != user.Id)
        {
            return Task.CompletedTask;
        }

        return context.SignInAsync(
            scheme: IdentityConstants.ApplicationScheme,
            principal: AuthenticatedUserPrincipalFactory.Create(user),
            properties: session.Properties);
    }

    internal sealed record Snapshot(Guid UserId, AuthenticationProperties Properties);
}
