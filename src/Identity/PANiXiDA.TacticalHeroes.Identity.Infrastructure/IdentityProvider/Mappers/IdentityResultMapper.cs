using Microsoft.AspNetCore.Identity;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Mappers;

internal static class IdentityResultMapper
{
    public static Result ToResult(IdentityResult result)
    {
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(errors: result.Errors.Select(MapError));
    }

    public static Result<TValue> ToResult<TValue>(IdentityResult result)
    {
        return result.Succeeded
            ? throw new InvalidOperationException(
                message: "A successful identity result must be mapped with an explicit value.")
            : Result.Failure<TValue>(errors: result.Errors.Select(MapError));
    }

    private static Error MapError(IdentityError error)
    {
        return error.Code switch
        {
            nameof(IdentityErrorDescriber.DuplicateEmail) or
            nameof(IdentityErrorDescriber.DuplicateUserName) or
            nameof(IdentityErrorDescriber.DuplicateRoleName) => Error.Conflict(message: error.Description),
            _ => Error.Validation(message: error.Description)
        };
    }
}
