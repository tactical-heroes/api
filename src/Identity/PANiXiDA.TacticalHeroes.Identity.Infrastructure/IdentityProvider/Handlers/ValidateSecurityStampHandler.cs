using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

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
        if (context.Principal?.GetTokenType() != OpenIddictConstants.TokenTypeIdentifiers.AccessToken
            || context.Principal.GetAuthorizationId() is null
            || !Guid.TryParse(context.Principal.GetClaim(OpenIddictConstants.Claims.Subject), out var userId))
        {
            return;
        }

        var securityStamp = await dbContext.Users
            .AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => user.SecurityStamp)
            .SingleOrDefaultAsync(context.CancellationToken);
        var tokenStamp = context.Principal.GetClaim(identityOptions.Value.ClaimsIdentity.SecurityStampClaimType);
        if (string.IsNullOrEmpty(securityStamp)
            || !string.Equals(tokenStamp, securityStamp, StringComparison.Ordinal))
        {
            context.Reject(
                error: OpenIddictConstants.Errors.InvalidToken,
                description: "User security stamp is invalid.");
        }
    }
}
