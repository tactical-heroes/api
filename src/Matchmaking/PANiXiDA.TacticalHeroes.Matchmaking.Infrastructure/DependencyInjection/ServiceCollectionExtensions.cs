using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Clients.DependencyInjection;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Streaming.DependencyInjection;

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

        serviceCollection.AddClients();
        serviceCollection.AddMatchSearchStreaming(configuration);

        return serviceCollection;
    }
}
