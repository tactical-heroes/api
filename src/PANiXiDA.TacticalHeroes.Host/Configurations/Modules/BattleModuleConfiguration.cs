using PANiXiDA.TacticalHeroes.Battle.Infrastructure.DependencyInjection;

namespace PANiXiDA.TacticalHeroes.Host.Configurations.Modules;

internal static class BattleModuleConfiguration
{
    internal static WebApplicationBuilder AddBattleModule(
        this WebApplicationBuilder builder)
    {
        builder.Services.AddInfrastructure(builder.Configuration);

        return builder;
    }
}
