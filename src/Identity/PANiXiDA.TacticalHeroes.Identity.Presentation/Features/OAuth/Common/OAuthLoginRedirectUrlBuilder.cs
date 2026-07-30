namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Common;

internal static class OAuthLoginRedirectUrlBuilder
{
    internal static string Build(
        string loginUrl,
        string returnUrl)
    {
        var builder = new UriBuilder(uri: loginUrl);
        var existingQuery = builder.Query.TrimStart(trimChar: '?');
        var encodedReturnUrl = Uri.EscapeDataString(stringToEscape: returnUrl);

        builder.Query = string.IsNullOrWhiteSpace(value: existingQuery)
            ? $"returnUrl={encodedReturnUrl}"
            : $"{existingQuery}&returnUrl={encodedReturnUrl}";

        return builder.Uri.ToString();
    }
}
