using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Modules.Notifications.Infrastructure.Email.DependencyInjection;

namespace PANiXiDA.TacticalHeroes.Modules.Notifications.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.AddEmail(configuration);

        return serviceCollection;
    }
}
