using PANiXiDA.TacticalHeroes.Players.Infrastructure.DependencyInjection;
using PANiXiDA.TacticalHeroes.Players.Presentation.DependencyInjection;

namespace PANiXiDA.TacticalHeroes.Host.Configurations.Modules;

internal static class PlayersModuleConfiguration
{
    internal static WebApplicationBuilder AddPlayersModule(
        this WebApplicationBuilder builder)
    {
        builder.Services.AddPresentation();
        builder.Services.AddInfrastructure(builder.Configuration);

        return builder;
    }
}
