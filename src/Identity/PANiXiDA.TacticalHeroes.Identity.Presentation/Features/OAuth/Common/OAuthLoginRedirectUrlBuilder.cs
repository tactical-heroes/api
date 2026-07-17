namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Common;

internal static class OAuthLoginRedirectUrlBuilder
{
    internal static string Build(
        string loginUrl,
        string returnUrl)
    {
        var builder = new UriBuilder(loginUrl);
        var existingQuery = builder.Query.TrimStart('?');
        var encodedReturnUrl = Uri.EscapeDataString(returnUrl);

        builder.Query = string.IsNullOrWhiteSpace(existingQuery)
            ? $"returnUrl={encodedReturnUrl}"
            : $"{existingQuery}&returnUrl={encodedReturnUrl}";

        return builder.Uri.ToString();
    }
}
