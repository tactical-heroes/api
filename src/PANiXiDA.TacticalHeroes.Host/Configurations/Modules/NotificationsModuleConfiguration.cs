using PANiXiDA.TacticalHeroes.Modules.Notifications.Infrastructure.DependencyInjection;
using PANiXiDA.TacticalHeroes.Modules.Notifications.Presentation.DependencyInjection;

namespace PANiXiDA.TacticalHeroes.Host.Configurations.Modules;

internal static class NotificationsModuleConfiguration
{
    internal static WebApplicationBuilder AddNotificationsModule(
        this WebApplicationBuilder builder)
    {
        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddPresentation();
        builder.Host.UseInfrastructure();

        return builder;
    }
}
