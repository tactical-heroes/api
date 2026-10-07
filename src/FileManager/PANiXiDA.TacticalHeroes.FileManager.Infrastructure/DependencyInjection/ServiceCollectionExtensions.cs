using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        return serviceCollection;
    }
}
