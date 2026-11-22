using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Matchmaking.Application.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Features.MatchSearches.Streaming.State;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Presentation.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddScoped<IMatchSearchStateNotifier, MatchSearchStateNotifier>();

        return services;
    }
}
