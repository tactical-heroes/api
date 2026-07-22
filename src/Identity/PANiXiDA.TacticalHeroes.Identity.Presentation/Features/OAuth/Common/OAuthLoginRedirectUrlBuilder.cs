namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Common;

internal static class OAuthLoginRedirectUrlBuilder
{
    internal static string Build(
        string? redirectUri,
        string returnUrl)
    {
        if (!Uri.TryCreate(redirectUri, UriKind.Absolute, out var callbackUri) ||
            !IsHttpScheme(callbackUri))
        {
            throw new InvalidOperationException("The validated OAuth redirect URI is unavailable.");
        }

        var builder = new UriBuilder(callbackUri)
        {
            Path = "/login",
            Query = $"returnUrl={Uri.EscapeDataString(returnUrl)}",
            Fragment = string.Empty
        };

        return builder.Uri.ToString();
    }

    private static bool IsHttpScheme(Uri uri)
    {
        return string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
    }
}
