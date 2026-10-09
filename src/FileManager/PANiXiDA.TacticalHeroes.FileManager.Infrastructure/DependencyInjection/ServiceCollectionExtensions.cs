using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Infrastructure.Storage.S3.DependencyInjection;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.AddPostgreSqlEfRepository<
            FileManagerWriteDbContext,
            FileManagerReadDbContext>(
            configuration);
        serviceCollection.AddS3FileStorage(configuration.GetRequiredSection(nameof(FileManager)));

        return serviceCollection;
    }
}
