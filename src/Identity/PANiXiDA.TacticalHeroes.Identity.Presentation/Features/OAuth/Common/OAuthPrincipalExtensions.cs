using System.Security.Claims;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Common;

internal static class OAuthPrincipalExtensions
{
    internal static Result<Guid> GetSubjectId(this ClaimsPrincipal? principal)
    {
        var accountIdValue = principal?.GetClaim(OpenIddictConstants.Claims.Subject);

        return Guid.TryParse(accountIdValue, out var accountId)
            ? Result.Success(accountId)
            : Result.Failure<Guid>(Error.Validation("Account id is required."));
    }
}
