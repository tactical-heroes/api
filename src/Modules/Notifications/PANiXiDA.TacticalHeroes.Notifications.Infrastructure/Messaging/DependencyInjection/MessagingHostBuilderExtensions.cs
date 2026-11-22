using System.Reflection;

using Microsoft.Extensions.Hosting;

using PANiXiDA.TacticalHeroes.Modules.Notifications.Application;
using PANiXiDA.TacticalHeroes.Modules.Notifications.Application.Abstractions.Email;

using Wolverine;

namespace PANiXiDA.TacticalHeroes.Modules.Notifications.Infrastructure.Messaging.DependencyInjection;

internal static class MessagingHostBuilderExtensions
{
    public static IHostBuilder UseMessaging(this IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureServices(services =>
        {
            services.ConfigureWolverine(options =>
            {
                options.Discovery.IncludeAssembly(ApplicationAssembly.Instance);
                options.Discovery.IncludeAssembly(Assembly.GetExecutingAssembly());
                options.CodeGeneration.AlwaysUseServiceLocationFor<IEmailSender>();
            });
        });

        return hostBuilder;
    }
}
