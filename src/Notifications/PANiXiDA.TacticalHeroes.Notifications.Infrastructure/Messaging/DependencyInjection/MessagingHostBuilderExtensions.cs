using Microsoft.Extensions.Hosting;

using PANiXiDA.TacticalHeroes.Notifications.Application;
using PANiXiDA.TacticalHeroes.Notifications.Application.Abstractions.Email;

using System.Reflection;

using Wolverine;

namespace PANiXiDA.TacticalHeroes.Notifications.Infrastructure.Messaging.DependencyInjection;

internal static class MessagingHostBuilderExtensions
{
    public static IHostBuilder UseMessaging(this IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureServices(configureDelegate: services =>
        {
            services.ConfigureWolverine(configure: options =>
            {
                options.Discovery.IncludeAssembly(assembly: ApplicationAssembly.Instance);
                options.Discovery.IncludeAssembly(assembly: Assembly.GetExecutingAssembly());
                options.CodeGeneration.AlwaysUseServiceLocationFor<IEmailSender>();
            });
        });

        return hostBuilder;
    }
}
