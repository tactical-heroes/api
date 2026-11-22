using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Modules.Players.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Modules.Players.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.AddPostgreSqlEfRepository<
            PlayersWriteDbContext,
            PlayersReadDbContext>(
                configuration);

        return serviceCollection;
    }
}
