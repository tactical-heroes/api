using PANiXiDA.TacticalHeroes.Modules.Builds.Infrastructure.DependencyInjection;
using PANiXiDA.TacticalHeroes.Modules.Builds.Presentation.DependencyInjection;

namespace PANiXiDA.TacticalHeroes.Host.Configurations.Modules;

internal static class BuildsModuleConfiguration
{
    internal static WebApplicationBuilder AddBuildsModule(
        this WebApplicationBuilder builder)
    {
        builder.Services.AddPresentation();
        builder.Services.AddInfrastructure(builder.Configuration);

        return builder;
    }
}
