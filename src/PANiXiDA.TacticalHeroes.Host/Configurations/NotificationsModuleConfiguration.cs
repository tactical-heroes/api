using PANiXiDA.TacticalHeroes.Notifications.Infrastructure.DependencyInjection;
using PANiXiDA.TacticalHeroes.Notifications.Presentation.DependencyInjection;

namespace PANiXiDA.TacticalHeroes.Host.Configurations;

internal static class NotificationsModuleConfiguration
{
    internal static WebApplicationBuilder AddNotificationsModule(
        this WebApplicationBuilder builder)
    {
        builder.Services.AddInfrastructure(
            configuration: builder.Configuration,
            environment: builder.Environment);
        builder.Services.AddPresentation(configuration: builder.Configuration);
        builder.Host.UseInfrastructure(configuration: builder.Configuration);

        return builder;
    }
}
