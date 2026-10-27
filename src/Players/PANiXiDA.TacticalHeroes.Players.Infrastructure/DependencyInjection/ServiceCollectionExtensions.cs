using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using PANiXiDA.TacticalHeroes.Players.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Players.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.TryAddSingleton(TimeProvider.System);

        serviceCollection.AddPostgreSqlEfRepository<
            PlayersWriteDbContext,
            PlayersReadDbContext>(
                configuration);

        return serviceCollection;
    }
}
