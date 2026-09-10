using PANiXiDA.TacticalHeroes.Notifications.Infrastructure.DependencyInjection;
using PANiXiDA.TacticalHeroes.Notifications.Presentation.DependencyInjection;

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
