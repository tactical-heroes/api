using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

using PANiXiDA.TacticalHeroes.Matchmaking.Application.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Tracking;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Streaming.DependencyInjection;

internal static class StreamingServiceCollectionExtensions
{
    internal static IServiceCollection AddMatchSearchStreaming(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(EfConstants.PostgreSqlConnectionStringName)
            ?? throw new InvalidOperationException("PostgreSQL connection string was not found.");

        services.TryAddEnumerable(ServiceDescriptor.Scoped<IInterceptor, MatchSearchStateVersionInterceptor>());
        services.TryAddSingleton<MatchSearchConnections>();
        services.AddSingleton<IMatchSearchStateFeed, MatchSearchStateFeed>();
        services.AddScoped<IMatchSearchStatePublisher, MatchSearchStatePublisher>();
        services.AddHostedService(provider => new MatchSearchStateListener(
            connectionString: connectionString,
            connections: provider.GetRequiredService<MatchSearchConnections>(),
            logger: provider.GetRequiredService<ILogger<MatchSearchStateListener>>()));

        return services;
    }
}
