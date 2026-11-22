using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Builds.Contracts.Builds;
using PANiXiDA.TacticalHeroes.Builds.Presentation.Internal;

namespace PANiXiDA.TacticalHeroes.Builds.Presentation.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentation(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IBuildsApi, BuildsApi>();

        return serviceCollection;
    }
}
