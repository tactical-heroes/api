using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.AddPostgreSqlEfRepository<
            MatchmakingWriteDbContext,
            MatchmakingReadDbContext>(
                configuration);

        return serviceCollection;
    }
}
