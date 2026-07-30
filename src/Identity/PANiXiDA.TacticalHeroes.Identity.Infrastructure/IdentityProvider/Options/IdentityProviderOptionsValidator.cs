using Microsoft.Extensions.Options;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options;

internal sealed class IdentityProviderOptionsValidator
    : IValidateOptions<IdentityProviderOptions>
{
    public ValidateOptionsResult Validate(
        string? name,
        IdentityProviderOptions options)
    {
        List<string> failures = [];

        if (options.Issuer is null)
        {
            failures.Add($"{IdentityProviderOptions.SectionName}:Issuer must be configured.");
        }
        else if (!options.Issuer.IsAbsoluteUri || !IsHttpScheme(uri: options.Issuer))
        {
            failures.Add(
                $"{IdentityProviderOptions.SectionName}:Issuer must be an absolute HTTP or HTTPS URI.");
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            failures.Add($"{IdentityProviderOptions.SectionName}:Audience must not be empty.");
        }

        ValidateLifetimes(options: options, failures: failures);

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures: failures);
    }

    private static void ValidateLifetimes(
        IdentityProviderOptions options,
        List<string> failures)
    {
        ValidatePositive(
            value: options.AccessTokenLifetime,
            path: $"{IdentityProviderOptions.SectionName}:AccessTokenLifetime",
            failures: failures);
        ValidatePositive(
            value: options.RefreshTokenLifetime,
            path: $"{IdentityProviderOptions.SectionName}:RefreshTokenLifetime",
            failures: failures);
        ValidateNonNegative(
            value: options.RefreshTokenReuseLeeway,
            path: $"{IdentityProviderOptions.SectionName}:RefreshTokenReuseLeeway",
            failures: failures);
        ValidatePositive(
            value: options.AuthorizationCodeLifetime,
            path: $"{IdentityProviderOptions.SectionName}:AuthorizationCodeLifetime",
            failures: failures);
        ValidatePositive(
            value: options.IdentityTokenLifetime,
            path: $"{IdentityProviderOptions.SectionName}:IdentityTokenLifetime",
            failures: failures);
        ValidatePositive(
            value: options.EmailConfirmationTokenLifetime,
            path: $"{IdentityProviderOptions.SectionName}:EmailConfirmationTokenLifetime",
            failures: failures);
        ValidatePositive(
            value: options.PasswordResetTokenLifetime,
            path: $"{IdentityProviderOptions.SectionName}:PasswordResetTokenLifetime",
            failures: failures);
    }

    private static void ValidatePositive(
        TimeSpan value,
        string path,
        List<string> failures)
    {
        if (value <= TimeSpan.Zero)
        {
            failures.Add($"{path} must be positive.");
        }
    }

    private static void ValidateNonNegative(
        TimeSpan value,
        string path,
        List<string> failures)
    {
        if (value < TimeSpan.Zero)
        {
            failures.Add($"{path} must not be negative.");
        }
    }

    private static bool IsHttpScheme(Uri uri)
    {
        return string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
    }
}
