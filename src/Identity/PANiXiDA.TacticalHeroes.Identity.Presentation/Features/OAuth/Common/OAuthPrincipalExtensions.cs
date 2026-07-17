using System.Security.Claims;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Common;

internal static class OAuthPrincipalExtensions
{
    internal static Result<Guid> GetSubjectId(this ClaimsPrincipal? principal)
    {
        var accountIdValue = principal?.GetClaim(OpenIddictConstants.Claims.Subject);

        return Guid.TryParse(input: accountIdValue, result: out var accountId)
            ? Result.Success(value: accountId)
            : Result.Failure<Guid>(error: Error.Validation(message: "Account id is required."));
    }
}
