using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using PANiXiDA.TacticalHeroes.Notifications.Application;

using System.Reflection;

using Wolverine;

namespace PANiXiDA.TacticalHeroes.Notifications.Infrastructure.DependencyInjection;

public static class HostBuilderExtensions
{
    public static IHostBuilder UseInfrastructure(
        this IHostBuilder hostBuilder,
        IConfiguration configuration)
    {
        hostBuilder.ConfigureServices(services =>
        {
            services.ConfigureWolverine(options =>
            {
                options.Discovery.IncludeAssembly(ApplicationAssembly.Instance);
                options.Discovery.IncludeAssembly(Assembly.GetExecutingAssembly());
            });
        });

        return hostBuilder;
    }
}
