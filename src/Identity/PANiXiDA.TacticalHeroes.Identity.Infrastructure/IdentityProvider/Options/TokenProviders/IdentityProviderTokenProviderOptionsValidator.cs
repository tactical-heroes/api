using Microsoft.Extensions.Options;

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
            if (string.IsNullOrWhiteSpace(value: options.TokenProviders.EmailConfirmation))
            {
                failures.Add(
                    item: $"{IdentityProviderOptions.SectionName}:TokenProviders:EmailConfirmation must not be empty.");
            }

            if (string.IsNullOrWhiteSpace(value: options.TokenProviders.PasswordReset))
            {
                failures.Add(
                    item: $"{IdentityProviderOptions.SectionName}:TokenProviders:PasswordReset must not be empty.");
            }

            if (string.Equals(
                a: options.TokenProviders.EmailConfirmation,
                b: options.TokenProviders.PasswordReset,
                comparisonType: StringComparison.Ordinal))
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
