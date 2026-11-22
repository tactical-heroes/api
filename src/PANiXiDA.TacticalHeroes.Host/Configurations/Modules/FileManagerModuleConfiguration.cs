using PANiXiDA.TacticalHeroes.Modules.FileManager.Infrastructure.DependencyInjection;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Presentation.DependencyInjection;

namespace PANiXiDA.TacticalHeroes.Host.Configurations.Modules;

internal static class FileManagerModuleConfiguration
{
    internal static WebApplicationBuilder AddFileManagerModule(
        this WebApplicationBuilder builder)
    {
        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddPresentation();

        return builder;
    }
}
