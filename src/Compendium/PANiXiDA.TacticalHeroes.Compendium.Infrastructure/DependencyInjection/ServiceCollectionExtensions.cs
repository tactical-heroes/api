using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.AddPostgreSqlWriteEfRepository<CompendiumWriteDbContext>(
            configuration);

        return serviceCollection;
    }
}
