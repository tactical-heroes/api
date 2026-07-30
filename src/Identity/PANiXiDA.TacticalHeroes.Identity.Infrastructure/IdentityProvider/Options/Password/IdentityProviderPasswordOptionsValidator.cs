using Microsoft.Extensions.Options;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options.Password;

internal sealed class IdentityProviderPasswordOptionsValidator
    : IValidateOptions<IdentityProviderOptions>
{
    public ValidateOptionsResult Validate(
        string? name,
        IdentityProviderOptions options)
    {
        List<string> failures = [];

        if (options.Password is null)
        {
            failures.Add($"{IdentityProviderOptions.SectionName}:Password must be configured.");
        }
        else
        {
            if (options.Password.RequiredLength <= 0)
            {
                failures.Add($"{IdentityProviderOptions.SectionName}:Password:RequiredLength must be positive.");
            }

            if (options.Password.RequiredUniqueChars < 0 ||
                options.Password.RequiredUniqueChars > options.Password.RequiredLength)
            {
                failures.Add(
                    $"{IdentityProviderOptions.SectionName}:Password:RequiredUniqueChars must be between 0 and RequiredLength.");
            }
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures: failures);
    }
}
