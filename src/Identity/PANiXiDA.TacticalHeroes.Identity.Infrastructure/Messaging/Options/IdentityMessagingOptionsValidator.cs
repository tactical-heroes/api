using Microsoft.Extensions.Options;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Messaging.Options;

internal sealed class IdentityMessagingOptionsValidator
    : IValidateOptions<IdentityMessagingOptions>
{
    public ValidateOptionsResult Validate(
        string? name,
        IdentityMessagingOptions options)
    {
        List<string> failures = [];

        ValidateTemplate(
            template: options.EmailConfirmationUrlTemplate,
            path: $"{IdentityMessagingOptions.SectionName}:EmailConfirmationUrlTemplate",
            failures: failures);
        ValidateTemplate(
            template: options.PasswordResetUrlTemplate,
            path: $"{IdentityMessagingOptions.SectionName}:PasswordResetUrlTemplate",
            failures: failures);

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures: failures);
    }

    private static void ValidateTemplate(
        string template,
        string path,
        ICollection<string> failures)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            failures.Add($"{path} must not be empty.");
            return;
        }

        if (!template.Contains("{userId}", StringComparison.Ordinal))
        {
            failures.Add($"{path} must contain the '{{userId}}' placeholder.");
        }

        if (!template.Contains("{token}", StringComparison.Ordinal))
        {
            failures.Add($"{path} must contain the '{{token}}' placeholder.");
        }

        var sampleUrl = template
            .Replace("{userId}", Guid.Empty.ToString("D"), StringComparison.Ordinal)
            .Replace("{token}", "token", StringComparison.Ordinal);

        if (!Uri.TryCreate(
            uriString: sampleUrl,
            uriKind: UriKind.RelativeOrAbsolute,
            result: out var uri) ||
            uri.IsAbsoluteUri && !IsHttpScheme(uri: uri) ||
            !uri.IsAbsoluteUri && !sampleUrl.StartsWith("/", StringComparison.Ordinal))
        {
            failures.Add($"{path} must be a root-relative or absolute HTTP/HTTPS URL template.");
        }
    }

    private static bool IsHttpScheme(Uri uri)
    {
        return string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
    }
}
