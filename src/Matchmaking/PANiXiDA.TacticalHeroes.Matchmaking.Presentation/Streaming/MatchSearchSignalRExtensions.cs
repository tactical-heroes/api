using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Matchmaking.Application.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Features.MatchSearches.Watch;

using StackExchange.Redis;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Streaming;

public static class MatchSearchSignalRExtensions
{
    public static IServiceCollection AddMatchSearchSignalR(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("Redis connection string was not found.");
        services.AddSignalR()
            .AddStackExchangeRedis(
                connectionString,
                options => options.Configuration.ChannelPrefix = RedisChannel.Literal("tactical-heroes:matchmaking"));
        services.AddScoped<IMatchSearchStateSubscription, MatchSearchStateSubscription>();
        services.AddScoped<IMatchSearchStatePublisher, MatchSearchStatePublisher>();

        return services;
    }

    public static IApplicationBuilder UseMatchSearchSignalRAuthentication(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            var request = context.Request;
            if (string.Equals(
                a: request.Path.Value?.TrimEnd('/'),
                b: MatchSearchHub.Route,
                comparisonType: StringComparison.OrdinalIgnoreCase)
                && !request.Headers.ContainsKey("Authorization")
                && request.Query.TryGetValue("access_token", out var token)
                && token.Count == 1)
            {
                request.Headers.Authorization = $"Bearer {token[0]}";
            }

            await next(context);
        });
    }
}
