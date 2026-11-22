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
        serviceCollection.TryAddSingleton(TimeProvider.System);

        serviceCollection.AddWritePersistence(configuration);
        serviceCollection.AddReadPersistence(configuration);

        serviceCollection.AddScheduling(configuration);
        serviceCollection.AddIdentityProvider(configuration, environment);
        serviceCollection.AddMessaging(configuration);

        return serviceCollection;
    }
}
