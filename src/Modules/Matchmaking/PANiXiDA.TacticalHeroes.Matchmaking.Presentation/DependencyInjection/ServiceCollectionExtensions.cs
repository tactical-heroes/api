using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Modules.Matchmaking.Application.Abstractions;
using PANiXiDA.TacticalHeroes.Modules.Matchmaking.Presentation.Features.MatchSearches.Streaming.State;

namespace PANiXiDA.TacticalHeroes.Modules.Matchmaking.Presentation.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddScoped<IMatchSearchStateNotifier, MatchSearchStateNotifier>();

        return services;
    }
}
