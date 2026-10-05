using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.DependencyInjection;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Messaging.DependencyInjection;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.DependencyInjection;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Scheduling.DependencyInjection;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection serviceCollection,
        IConfiguration configuration,
        IHostEnvironment? environment = null)
    {
        serviceCollection.TryAddSingleton(instance: TimeProvider.System);

        serviceCollection.AddWritePersistence(configuration: configuration);
        serviceCollection.AddReadPersistence(configuration: configuration);

        serviceCollection.AddScheduling(configuration: configuration);
        serviceCollection.AddIdentityProvider(configuration: configuration, environment: environment);
        serviceCollection.AddMessaging(configuration: configuration);

        return serviceCollection;
    }
}
