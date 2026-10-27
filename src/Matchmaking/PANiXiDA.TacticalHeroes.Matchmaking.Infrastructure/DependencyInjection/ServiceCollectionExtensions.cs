using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Matchmaking.Application.Builds;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.Players;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Clients;
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

        serviceCollection.AddScoped<IPlayersClient, PlayersClient>();
        serviceCollection.AddScoped<IBuildsClient, BuildsClient>();

        return serviceCollection;
    }
}
