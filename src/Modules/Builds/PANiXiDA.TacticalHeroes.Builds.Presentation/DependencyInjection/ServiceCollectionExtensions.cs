using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Modules.Builds.Contracts.Builds;
using PANiXiDA.TacticalHeroes.Modules.Builds.Presentation.Internal;

namespace PANiXiDA.TacticalHeroes.Modules.Builds.Presentation.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentation(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IBuildsApi, BuildsApi>();

        return serviceCollection;
    }
}
