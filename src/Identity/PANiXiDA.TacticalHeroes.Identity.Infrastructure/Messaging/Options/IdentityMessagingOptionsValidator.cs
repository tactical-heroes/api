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
        if (string.IsNullOrWhiteSpace(value: template))
        {
            failures.Add(item: $"{path} must not be empty.");
            return;
        }

        if (!template.Contains(value: "{userId}", comparisonType: StringComparison.Ordinal))
        {
            failures.Add(item: $"{path} must contain the '{{userId}}' placeholder.");
        }

        if (!template.Contains(value: "{token}", comparisonType: StringComparison.Ordinal))
        {
            failures.Add(item: $"{path} must contain the '{{token}}' placeholder.");
        }

        var sampleUrl = template
            .Replace(oldValue: "{userId}", newValue: Guid.Empty.ToString(format: "D"), comparisonType: StringComparison.Ordinal)
            .Replace(oldValue: "{token}", newValue: "token", comparisonType: StringComparison.Ordinal);

        if (!Uri.TryCreate(
            uriString: sampleUrl,
            uriKind: UriKind.RelativeOrAbsolute,
            result: out var uri) ||
            uri.IsAbsoluteUri && !IsHttpScheme(uri: uri) ||
            !uri.IsAbsoluteUri && !sampleUrl.StartsWith(value: "/", comparisonType: StringComparison.Ordinal))
        {
            failures.Add(item: $"{path} must be a root-relative or absolute HTTP/HTTPS URL template.");
        }
    }

    private static bool IsHttpScheme(Uri uri)
    {
        return string.Equals(a: uri.Scheme, b: Uri.UriSchemeHttp, comparisonType: StringComparison.OrdinalIgnoreCase) ||
               string.Equals(a: uri.Scheme, b: Uri.UriSchemeHttps, comparisonType: StringComparison.OrdinalIgnoreCase);
    }
}
