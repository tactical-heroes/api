using Microsoft.Extensions.Hosting;

using PANiXiDA.TacticalHeroes.Notifications.Infrastructure.Messaging.DependencyInjection;

namespace PANiXiDA.TacticalHeroes.Notifications.Infrastructure.DependencyInjection;

public static class HostBuilderExtensions
{
    public static IHostBuilder UseInfrastructure(this IHostBuilder hostBuilder)
    {
        return hostBuilder.UseMessaging();
    }
}
