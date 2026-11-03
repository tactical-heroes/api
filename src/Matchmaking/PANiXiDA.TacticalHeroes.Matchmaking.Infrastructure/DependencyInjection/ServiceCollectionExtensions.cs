using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Clients.DependencyInjection;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Tracking;

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
        serviceCollection.TryAddEnumerable(
            ServiceDescriptor.Scoped<IInterceptor, MatchSearchStateVersionInterceptor>());

        return serviceCollection;
    }
}
