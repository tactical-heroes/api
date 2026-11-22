using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Modules.Compendium.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Modules.Compendium.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.AddPostgreSqlEfRepository<
            CompendiumWriteDbContext,
            CompendiumReadDbContext>(
                configuration);

        return serviceCollection;
    }
}
