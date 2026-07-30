using Microsoft.AspNetCore.Identity;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Mappers;

[Mapper]
internal static partial class IdentityResultMapper
{
    [MapperIgnore]
    public static Result ToResult(IdentityResult result)
    {
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(errors: ToErrors(errors: result.Errors));
    }

    [MapperIgnore]
    public static Result<TValue> ToResult<TValue>(IdentityResult result)
    {
        return result.Succeeded
            ? throw new InvalidOperationException(
                message: "A successful identity result must be mapped with an explicit value.")
            : Result.Failure<TValue>(errors: ToErrors(errors: result.Errors));
    }

    private static partial IReadOnlyCollection<Error> ToErrors(
        IEnumerable<IdentityError> errors);

    [UserMapping(Default = true)]
    private static Error ToError(IdentityError error)
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
