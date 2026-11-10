using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Compendium.Contracts;
using PANiXiDA.TacticalHeroes.Compendium.Presentation.Internal;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddScoped<ICompendiumApi, CompendiumApi>();

        return services;
    }
}
