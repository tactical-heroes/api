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
            !IsValid(url: url, httpContext: httpContext, allowedPath: allowedPath))
        {
            return Result.Failure(
                error: Error.Validation(message: "Return URL is invalid.")
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

        if (!Uri.TryCreate(uriString: url, uriKind: UriKind.Absolute, result: out var uri))
        {
            return false;
        }

        var request = httpContext.Request;
        var requestHost = request.Host.ToUriComponent();
        var redirectHost = uri.IsDefaultPort
            ? uri.Host
            : uri.GetComponents(UriComponents.HostAndPort, UriFormat.UriEscaped);

        return string.Equals(a: uri.Scheme, b: request.Scheme, comparisonType: StringComparison.OrdinalIgnoreCase) &&
            string.Equals(a: redirectHost, b: requestHost, comparisonType: StringComparison.OrdinalIgnoreCase) &&
            string.Equals(a: uri.AbsolutePath, b: allowedPath, comparisonType: StringComparison.Ordinal);
    }

    private static string GetPath(string pathAndQuery)
    {
        var queryStartIndex = pathAndQuery.IndexOf('?');

        return queryStartIndex < 0
            ? pathAndQuery
            : pathAndQuery[..queryStartIndex];
    }
}
