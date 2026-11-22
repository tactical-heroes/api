using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.DependencyInjection;
using PANiXiDA.TacticalHeroes.Compendium.Presentation.DependencyInjection;

namespace PANiXiDA.TacticalHeroes.Host.Configurations.Modules;

internal static class CompendiumModuleConfiguration
{
    internal static WebApplicationBuilder AddCompendiumModule(
        this WebApplicationBuilder builder)
    {
        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddPresentation();

        return builder;
    }
}
