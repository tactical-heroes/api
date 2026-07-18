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

        ValidateIssuer(options: options, failures: failures);
        ValidateLifetimes(options: options, failures: failures);
        ValidatePassword(options: options, failures: failures);
        ValidateLockout(options: options, failures: failures);
        ValidateTokenProviders(options: options, failures: failures);
        ValidateClients(options: options, failures: failures);

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            failures.Add($"{IdentityProviderOptions.SectionName}:Audience must not be empty.");
        }

        if (options.User is null)
        {
            failures.Add($"{IdentityProviderOptions.SectionName}:User must be configured.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures: failures);
    }

    private static void ValidateIssuer(
        IdentityProviderOptions options,
        ICollection<string> failures)
    {
        if (options.Issuer is null)
        {
            failures.Add($"{IdentityProviderOptions.SectionName}:Issuer must be configured.");
            return;
        }

        if (!options.Issuer.IsAbsoluteUri || !IsHttpScheme(uri: options.Issuer))
        {
            failures.Add(
                $"{IdentityProviderOptions.SectionName}:Issuer must be an absolute HTTP or HTTPS URI.");
        }
    }

    private static void ValidateLifetimes(
        IdentityProviderOptions options,
        ICollection<string> failures)
    {
        ValidatePositive(
            value: options.AccessTokenLifetime,
            path: $"{IdentityProviderOptions.SectionName}:AccessTokenLifetime",
            failures: failures);
        ValidatePositive(
            value: options.RefreshTokenLifetime,
            path: $"{IdentityProviderOptions.SectionName}:RefreshTokenLifetime",
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

    private static void ValidatePassword(
        IdentityProviderOptions options,
        ICollection<string> failures)
    {
        if (options.Password is null)
        {
            failures.Add($"{IdentityProviderOptions.SectionName}:Password must be configured.");
            return;
        }

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

    private static void ValidateLockout(
        IdentityProviderOptions options,
        ICollection<string> failures)
    {
        if (options.Lockout is null)
        {
            failures.Add($"{IdentityProviderOptions.SectionName}:Lockout must be configured.");
            return;
        }

        if (options.Lockout.MaxFailedAccessAttempts <= 0)
        {
            failures.Add(
                $"{IdentityProviderOptions.SectionName}:Lockout:MaxFailedAccessAttempts must be positive.");
        }

        ValidatePositive(
            value: options.Lockout.DefaultLockoutTimeSpan,
            path: $"{IdentityProviderOptions.SectionName}:Lockout:DefaultLockoutTimeSpan",
            failures: failures);
    }

    private static void ValidateTokenProviders(
        IdentityProviderOptions options,
        ICollection<string> failures)
    {
        if (options.TokenProviders is null)
        {
            failures.Add($"{IdentityProviderOptions.SectionName}:TokenProviders must be configured.");
            return;
        }

        if (string.IsNullOrWhiteSpace(options.TokenProviders.EmailConfirmation))
        {
            failures.Add(
                $"{IdentityProviderOptions.SectionName}:TokenProviders:EmailConfirmation must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(options.TokenProviders.PasswordReset))
        {
            failures.Add(
                $"{IdentityProviderOptions.SectionName}:TokenProviders:PasswordReset must not be empty.");
        }

        if (string.Equals(
            options.TokenProviders.EmailConfirmation,
            options.TokenProviders.PasswordReset,
            StringComparison.Ordinal))
        {
            failures.Add(
                $"{IdentityProviderOptions.SectionName}:TokenProviders values must be unique.");
        }
    }

    private static void ValidateClients(
        IdentityProviderOptions options,
        ICollection<string> failures)
    {
        if (options.Clients is null || options.Clients.Count == 0)
        {
            failures.Add($"{IdentityProviderOptions.SectionName}:Clients must contain at least one client.");
            return;
        }

        HashSet<string> clientIds = new(comparer: StringComparer.Ordinal);

        for (var index = 0; index < options.Clients.Count; index++)
        {
            var client = options.Clients[index];
            var path = $"{IdentityProviderOptions.SectionName}:Clients:{index}";

            if (client is null)
            {
                failures.Add($"{path} must be configured.");
                continue;
            }

            ValidateClient(
                client: client,
                path: path,
                clientIds: clientIds,
                failures: failures);
        }
    }

    private static void ValidateClient(
        IdentityProviderClientOptions client,
        string path,
        ISet<string> clientIds,
        ICollection<string> failures)
    {
        if (string.IsNullOrWhiteSpace(client.ClientId))
        {
            failures.Add($"{path}:ClientId must not be empty.");
        }
        else if (!clientIds.Add(client.ClientId))
        {
            failures.Add($"{path}:ClientId must be unique.");
        }

        if (string.IsNullOrWhiteSpace(client.DisplayName))
        {
            failures.Add($"{path}:DisplayName must not be empty.");
        }

        var isPublic = string.Equals(
            client.ClientType,
            OpenIddictConstants.ClientTypes.Public,
            StringComparison.Ordinal);
        var isConfidential = string.Equals(
            client.ClientType,
            OpenIddictConstants.ClientTypes.Confidential,
            StringComparison.Ordinal);

        if (!isPublic && !isConfidential)
        {
            failures.Add($"{path}:ClientType must be public or confidential.");
        }

        if (isConfidential && string.IsNullOrWhiteSpace(client.ClientSecret))
        {
            failures.Add($"{path}:ClientSecret must not be empty for a confidential client.");
        }

        ValidateRequiredValues(
            values: client.GrantTypes,
            path: $"{path}:GrantTypes",
            failures: failures);
        ValidateGrantTypes(
            values: client.GrantTypes,
            path: $"{path}:GrantTypes",
            failures: failures);
        ValidateOptionalValues(
            values: client.Scopes,
            path: $"{path}:Scopes",
            failures: failures);
        ValidateUris(
            values: client.RedirectUris,
            path: $"{path}:RedirectUris",
            failures: failures);
        ValidateUris(
            values: client.PostLogoutRedirectUris,
            path: $"{path}:PostLogoutRedirectUris",
            failures: failures);
    }

    private static void ValidateRequiredValues(
        IReadOnlyCollection<string>? values,
        string path,
        ICollection<string> failures)
    {
        if (values is null || values.Count == 0)
        {
            failures.Add($"{path} must contain at least one value.");
            return;
        }

        ValidateOptionalValues(values: values, path: path, failures: failures);
    }

    private static void ValidateOptionalValues(
        IReadOnlyCollection<string>? values,
        string path,
        ICollection<string> failures)
    {
        if (values is null)
        {
            failures.Add($"{path} must be configured.");
            return;
        }

        if (values.Any(string.IsNullOrWhiteSpace))
        {
            failures.Add($"{path} must not contain empty values.");
        }

        if (values.Distinct(StringComparer.Ordinal).Count() != values.Count)
        {
            failures.Add($"{path} must not contain duplicate values.");
        }
    }

    private static void ValidateGrantTypes(
        IReadOnlyCollection<string>? values,
        string path,
        ICollection<string> failures)
    {
        if (values is null)
        {
            return;
        }

        if (values.Any(grantType => grantType is not (
            OpenIddictConstants.GrantTypes.AuthorizationCode or
            OpenIddictConstants.GrantTypes.RefreshToken or
            OpenIddictConstants.GrantTypes.ClientCredentials or
            OpenIddictConstants.GrantTypes.TokenExchange)))
        {
            failures.Add($"{path} contains an unsupported OAuth grant type.");
        }
    }

    private static void ValidateUris(
        IReadOnlyCollection<string>? values,
        string path,
        ICollection<string> failures)
    {
        if (values is null)
        {
            failures.Add($"{path} must be configured.");
            return;
        }

        if (values.Any(value =>
            !Uri.TryCreate(uriString: value, uriKind: UriKind.Absolute, result: out _)))
        {
            failures.Add($"{path} must contain only absolute URIs.");
        }
    }

    private static void ValidatePositive(
        TimeSpan value,
        string path,
        ICollection<string> failures)
    {
        if (value <= TimeSpan.Zero)
        {
            failures.Add($"{path} must be positive.");
        }
    }

    private static bool IsHttpScheme(Uri uri)
    {
        return string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
    }
}
