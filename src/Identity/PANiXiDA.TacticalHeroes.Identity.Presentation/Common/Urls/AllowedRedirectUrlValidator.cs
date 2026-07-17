using Microsoft.AspNetCore.Http;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Common.Urls;

internal static class AllowedRedirectUrlValidator
{
    internal static Result Validate(
        string url,
        HttpContext httpContext,
        string allowedPath,
        string fieldName)
    {
        if (string.IsNullOrWhiteSpace(url) ||
            !IsValid(url, httpContext, allowedPath))
        {
            return Result.Failure(
                Error.Validation("Return URL is invalid.")
                    .WithField(fieldName));
        }

        return Result.Success();
    }

    private static bool IsValid(
        string url,
        HttpContext httpContext,
        string allowedPath)
    {
        if (url.StartsWith('/') &&
            !url.StartsWith("//", StringComparison.Ordinal))
        {
            return GetPath(url).Equals(allowedPath, StringComparison.Ordinal);
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return false;
        }

        var request = httpContext.Request;
        var requestHost = request.Host.ToUriComponent();
        var redirectHost = uri.IsDefaultPort
            ? uri.Host
            : uri.GetComponents(UriComponents.HostAndPort, UriFormat.UriEscaped);

        return string.Equals(uri.Scheme, request.Scheme, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(redirectHost, requestHost, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(uri.AbsolutePath, allowedPath, StringComparison.Ordinal);
    }

    private static string GetPath(string pathAndQuery)
    {
        var queryStartIndex = pathAndQuery.IndexOf('?');

        return queryStartIndex < 0
            ? pathAndQuery
            : pathAndQuery[..queryStartIndex];
    }
}
