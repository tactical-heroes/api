using Microsoft.Extensions.Options;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth;

internal sealed class OAuthSpaOptionsValidator
    : IValidateOptions<OAuthSpaOptions>
{
    public ValidateOptionsResult Validate(
        string? name,
        OAuthSpaOptions options)
    {
        if (Uri.TryCreate(
            uriString: options.LoginUrl,
            uriKind: UriKind.Absolute,
            result: out var loginUri) &&
            IsHttpScheme(uri: loginUri))
        {
            return ValidateOptionsResult.Success;
        }

        return ValidateOptionsResult.Fail(
            failureMessage: $"{OAuthSpaOptions.SectionName}:LoginUrl must be an absolute HTTP or HTTPS URI.");
    }

    private static bool IsHttpScheme(Uri uri)
    {
        return string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
    }
}
