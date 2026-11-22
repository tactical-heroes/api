using PANiXiDA.TacticalHeroes.Games.Infrastructure.DependencyInjection;

namespace PANiXiDA.TacticalHeroes.Host.Configurations.Modules;

internal static class GamesModuleConfiguration
{
    internal static WebApplicationBuilder AddGamesModule(
        this WebApplicationBuilder builder)
    {
        builder.Services.AddInfrastructure(builder.Configuration);

        return builder;
    }
}
