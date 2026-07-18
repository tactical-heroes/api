using Microsoft.Extensions.Options;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options.Clients;

internal sealed class IdentityProviderClientsOptionsValidator
    : IValidateOptions<IdentityProviderOptions>
{
    public ValidateOptionsResult Validate(
        string? name,
        IdentityProviderOptions options)
    {
        List<string> failures = [];

        if (options.Clients is null || options.Clients.Count == 0)
        {
            failures.Add($"{IdentityProviderOptions.SectionName}:Clients must contain at least one client.");
        }
        else
        {
            ValidateClients(clients: options.Clients, failures: failures);
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures: failures);
    }

    private static void ValidateClients(
        List<IdentityProviderClientOptions> clients,
        List<string> failures)
    {
        HashSet<string> clientIds = new(comparer: StringComparer.Ordinal);

        for (var index = 0; index < clients.Count; index++)
        {
            var client = clients[index];
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
        HashSet<string> clientIds,
        List<string> failures)
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
        List<string>? values,
        string path,
        List<string> failures)
    {
        if (values is null || values.Count == 0)
        {
            failures.Add($"{path} must contain at least one value.");
            return;
        }

        ValidateOptionalValues(values: values, path: path, failures: failures);
    }

    private static void ValidateOptionalValues(
        List<string>? values,
        string path,
        List<string> failures)
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
        List<string>? values,
        string path,
        List<string> failures)
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
        List<string>? values,
        string path,
        List<string> failures)
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
}
