using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options.IdentityProvider;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options.TokenProviders;

internal sealed class IdentityProviderTokenProviderOptionsValidator
    : IValidateOptions<IdentityProviderOptions>
{
    public ValidateOptionsResult Validate(
        string? name,
        IdentityProviderOptions options)
    {
        List<string> failures = [];

        if (options.TokenProviders is null)
        {
            failures.Add(item: $"{IdentityProviderOptions.SectionName}:TokenProviders must be configured.");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(options.TokenProviders.EmailConfirmation))
            {
                failures.Add(
                    item: $"{IdentityProviderOptions.SectionName}:TokenProviders:EmailConfirmation must not be empty.");
            }

            if (string.IsNullOrWhiteSpace(options.TokenProviders.PasswordReset))
            {
                failures.Add(
                    item: $"{IdentityProviderOptions.SectionName}:TokenProviders:PasswordReset must not be empty.");
            }

            if (string.Equals(
                options.TokenProviders.EmailConfirmation,
                options.TokenProviders.PasswordReset,
                StringComparison.Ordinal))
            {
                failures.Add(
                    item: $"{IdentityProviderOptions.SectionName}:TokenProviders values must be unique.");
            }
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures: failures);
    }
}
