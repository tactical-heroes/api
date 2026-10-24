using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PANiXiDA.TacticalHeroes.Battle.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(serviceCollection);
        ArgumentNullException.ThrowIfNull(configuration);

        return serviceCollection;
    }
}
