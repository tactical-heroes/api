using PANiXiDA.TacticalHeroes.Players.Infrastructure.DependencyInjection;

namespace PANiXiDA.TacticalHeroes.Host.Configurations.Modules;

internal static class PlayersModuleConfiguration
{
    internal static WebApplicationBuilder AddPlayersModule(
        this WebApplicationBuilder builder)
    {
        builder.Services.AddInfrastructure(builder.Configuration);

        return builder;
    }
}
