using System.Globalization;

using Microsoft.EntityFrameworkCore;

using OpenIddict.Server;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Claims;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Handlers;

internal sealed class ValidateRoleAssignments(IdentityWriteDbContext dbContext)
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

        var version = await dbContext.Users
            .AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => (long?)user.RoleAssignmentsVersion)
            .SingleOrDefaultAsync(context.CancellationToken);
        var tokenVersion = context.Principal.GetClaim(IdentityClaimsFactory.RoleAssignmentsVersionClaimType) ?? "0";
        if (version is null || tokenVersion != version.Value.ToString(CultureInfo.InvariantCulture))
        {
            context.Reject(
                error: OpenIddictConstants.Errors.InvalidToken,
                description: "User role assignments have changed.");
        }
    }
}
