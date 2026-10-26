using PANiXiDA.TacticalHeroes.Builds.Infrastructure.DependencyInjection;

namespace PANiXiDA.TacticalHeroes.Host.Configurations.Modules;

internal static class BuildsModuleConfiguration
{
    internal static WebApplicationBuilder AddBuildsModule(
        this WebApplicationBuilder builder)
    {
        builder.Services.AddInfrastructure(builder.Configuration);

        return builder;
    }
}
