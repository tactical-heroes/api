using System.Security.Claims;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Common;

internal static class OAuthPrincipalExtensions
{
    internal static Result<Guid> GetSubjectId(this ClaimsPrincipal? principal)
    {
        var userIdValue = principal?.GetClaim(OpenIddictConstants.Claims.Subject);

        return Guid.TryParse(input: userIdValue, result: out var userId)
            ? Result.Success(value: userId)
            : Result.Failure<Guid>(error: Error.Validation(message: "User id is required."));
    }
}
