using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Modules.Battle.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Modules.Battle.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.AddPostgreSqlEfRepository<
            BattleWriteDbContext,
            BattleReadDbContext>(
                configuration);

        return serviceCollection;
    }
}
