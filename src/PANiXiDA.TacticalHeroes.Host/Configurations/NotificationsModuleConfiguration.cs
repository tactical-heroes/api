using PANiXiDA.TacticalHeroes.Notifications.Infrastructure.DependencyInjection;
using PANiXiDA.TacticalHeroes.Notifications.Presentation.DependencyInjection;

namespace PANiXiDA.TacticalHeroes.Host.Configurations;

internal static class NotificationsModuleConfiguration
{
    internal static WebApplicationBuilder AddNotificationsModule(
        this WebApplicationBuilder builder)
    {
        builder.Services.AddInfrastructure(
            builder.Configuration,
            builder.Environment);
        builder.Services.AddPresentation(builder.Configuration);
        builder.Host.UseInfrastructure(builder.Configuration);

        return builder;
    }
}
