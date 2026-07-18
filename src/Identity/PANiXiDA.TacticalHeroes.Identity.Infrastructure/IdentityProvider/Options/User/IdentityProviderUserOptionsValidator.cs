using Microsoft.Extensions.Options;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options.User;

internal sealed class IdentityProviderUserOptionsValidator
    : IValidateOptions<IdentityProviderOptions>
{
    public ValidateOptionsResult Validate(
        string? name,
        IdentityProviderOptions options)
    {
        return options.User is null
            ? ValidateOptionsResult.Fail(
                failureMessage: $"{IdentityProviderOptions.SectionName}:User must be configured.")
            : ValidateOptionsResult.Success;
    }
}
