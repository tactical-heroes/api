using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Factions.Write;

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.DependencyInjection;

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
        serviceCollection.AddScoped<IFactionsRepository, FactionsRepository>();

        return serviceCollection;
    }
}
