using Microsoft.Extensions.Hosting;

using PANiXiDA.TacticalHeroes.Modules.Notifications.Infrastructure.Messaging.DependencyInjection;

namespace PANiXiDA.TacticalHeroes.Modules.Notifications.Infrastructure.DependencyInjection;

public static class HostBuilderExtensions
{
    public static IHostBuilder UseInfrastructure(
        this IHostBuilder hostBuilder)
    {
        return hostBuilder.UseMessaging();
    }
}
