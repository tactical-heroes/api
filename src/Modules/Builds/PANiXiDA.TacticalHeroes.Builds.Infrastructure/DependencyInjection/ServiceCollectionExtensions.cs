using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Modules.Builds.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Modules.Builds.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.AddPostgreSqlEfRepository<
            BuildsWriteDbContext,
            BuildsReadDbContext>(
                configuration);

        return serviceCollection;
    }
}
