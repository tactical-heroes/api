using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Streaming;

public static class MatchSearchWebSocketExtensions
{
    public const string Protocol = "match-search-state.v1";
    public const string CredentialPrefix = "bearer.";

    public static IApplicationBuilder UseMatchSearchWebSocketAuthentication(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            if (string.Equals(
                a: context.Request.Path.Value?.TrimEnd('/'),
                b: "/api/v1/match-searches/state",
                comparisonType: StringComparison.OrdinalIgnoreCase))
            {
                var protocols = context.Request.Headers.GetCommaSeparatedValues(HeaderNames.SecWebSocketProtocol);
                var credentials = protocols.Where(value => value.StartsWith(CredentialPrefix, StringComparison.Ordinal))
                    .ToArray();
                if (protocols.Contains(Protocol, StringComparer.Ordinal)
                    && credentials.Length == 1
                    && !context.Request.Headers.ContainsKey(HeaderNames.Authorization))
                {
                    context.Request.Headers.Authorization = $"Bearer {credentials[0][CredentialPrefix.Length..]}";
                }

                context.Request.Headers[HeaderNames.SecWebSocketProtocol] = protocols
                    .Where(value => !value.StartsWith(CredentialPrefix, StringComparison.Ordinal))
                    .ToArray();
            }

            await next(context);
        });
    }
}
