using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.DependencyInjection;
using PANiXiDA.TacticalHeroes.Matchmaking.Presentation.DependencyInjection;

namespace PANiXiDA.TacticalHeroes.Host.Configurations.Modules;

internal static class MatchmakingModuleConfiguration
{
    internal static WebApplicationBuilder AddMatchmakingModule(
        this WebApplicationBuilder builder)
    {
        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddPresentation();

        return builder;
    }
}
