using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using OpenIddict.EntityFrameworkCore.Models;
using OpenIddict.Server;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Handlers;

internal sealed class ValidateSecurityStampHandler(
    IdentityWriteDbContext dbContext,
    IOptions<IdentityOptions> identityOptions)
    : IOpenIddictServerHandler<OpenIddictServerEvents.ValidateTokenContext>
{
    public async ValueTask HandleAsync(OpenIddictServerEvents.ValidateTokenContext context)
    {
        var tokenType = context.Principal?.GetTokenType();
        if (tokenType is not (OpenIddictConstants.TokenTypeIdentifiers.AccessToken
                or OpenIddictConstants.TokenTypeIdentifiers.RefreshToken
                or OpenIddictConstants.TokenTypeIdentifiers.Private.AuthorizationCode)
            || context.Principal?.GetAuthorizationId() is not { } authorizationId
            || !Guid.TryParse(context.Principal.GetClaim(OpenIddictConstants.Claims.Subject), out var userId))
        {
            return;
        }

        if (!Guid.TryParse(authorizationId, out var id))
        {
            context.Reject(error: OpenIddictConstants.Errors.InvalidToken, description: "User session is invalid.");
            return;
        }

        var subject = userId.ToString();
        var session = await dbContext.Set<OpenIddictEntityFrameworkCoreAuthorization<Guid>>()
            .AsNoTracking()
            .Where(authorization => authorization.Id == id && authorization.Subject == subject)
            .Select(
                authorization => new
                {
                    authorization.Status,
                    SecurityStamp = dbContext.Users
                        .Where(user => user.Id == userId)
                        .Select(user => user.SecurityStamp)
                        .FirstOrDefault()
                })
            .SingleOrDefaultAsync(context.CancellationToken);
        if (session is null || session.Status != OpenIddictConstants.Statuses.Valid)
        {
            context.Reject(error: OpenIddictConstants.Errors.InvalidToken, description: "User session is invalid.");
            return;
        }

        if (tokenType != OpenIddictConstants.TokenTypeIdentifiers.AccessToken)
        {
            return;
        }

        var tokenStamp = context.Principal.GetClaim(identityOptions.Value.ClaimsIdentity.SecurityStampClaimType);
        if (string.IsNullOrEmpty(session.SecurityStamp)
            || !string.Equals(tokenStamp, session.SecurityStamp, StringComparison.Ordinal))
        {
            context.Reject(
                error: OpenIddictConstants.Errors.InvalidToken,
                description: "User security stamp is invalid.");
        }
    }
}
