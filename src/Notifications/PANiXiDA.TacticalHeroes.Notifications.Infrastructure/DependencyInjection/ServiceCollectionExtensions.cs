using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using PANiXiDA.TacticalHeroes.Notifications.Infrastructure.Email.DependencyInjection;

namespace PANiXiDA.TacticalHeroes.Notifications.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection serviceCollection,
        IConfiguration configuration,
        IHostEnvironment? environment = null)
    {
        serviceCollection.AddEmail(configuration);

        return serviceCollection;
    }
}
