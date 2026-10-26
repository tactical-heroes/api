using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using PANiXiDA.Core.Infrastructure.Storage.S3.DependencyInjection;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Write;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Scheduling.DependencyInjection;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Scheduling.Reconciliation;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Storage;

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
        serviceCollection.TryAddSingleton(TimeProvider.System);
        serviceCollection.AddScoped<S3FileMetadataReader>();
        serviceCollection.AddScoped<IFileUploadFinalizer, FileUploadFinalizer>();
        serviceCollection.AddScoped<IPersonalFileUploadReconciler, PersonalFileUploadReconciler>();
        serviceCollection.AddScheduling(configuration);

        return serviceCollection;
    }
}
