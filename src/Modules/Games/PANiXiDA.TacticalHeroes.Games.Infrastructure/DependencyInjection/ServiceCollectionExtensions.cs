using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Games.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Games.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.AddPostgreSqlEfRepository<
            GamesWriteDbContext,
            GamesReadDbContext>(
                configuration);

        return serviceCollection;
    }
}
