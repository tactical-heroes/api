using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.DependencyInjection;

namespace PANiXiDA.TacticalHeroes.Host.Configurations;

internal static class CompendiumModuleConfiguration
{
    internal static WebApplicationBuilder AddCompendiumModule(
        this WebApplicationBuilder builder)
    {
        builder.Services.AddInfrastructure(configuration: builder.Configuration);

        return builder;
    }
}
