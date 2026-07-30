using Microsoft.Extensions.Options;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options.Lockout;

internal sealed class IdentityProviderLockoutOptionsValidator
    : IValidateOptions<IdentityProviderOptions>
{
    public ValidateOptionsResult Validate(
        string? name,
        IdentityProviderOptions options)
    {
        List<string> failures = [];

        if (options.Lockout is null)
        {
            failures.Add($"{IdentityProviderOptions.SectionName}:Lockout must be configured.");
        }
        else
        {
            if (options.Lockout.MaxFailedAccessAttempts <= 0)
            {
                failures.Add(
                    $"{IdentityProviderOptions.SectionName}:Lockout:MaxFailedAccessAttempts must be positive.");
            }

            if (options.Lockout.DefaultLockoutTimeSpan <= TimeSpan.Zero)
            {
                failures.Add(
                    $"{IdentityProviderOptions.SectionName}:Lockout:DefaultLockoutTimeSpan must be positive.");
            }
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures: failures);
    }
}
