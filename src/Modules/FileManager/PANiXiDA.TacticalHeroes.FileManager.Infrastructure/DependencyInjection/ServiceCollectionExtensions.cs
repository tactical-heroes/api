using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using PANiXiDA.Core.Infrastructure.Storage.S3.DependencyInjection;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Options.FileRelocation;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Services;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Services;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Options.FileRelocation;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Folders.Write;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Scheduling.DependencyInjection;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.AddSingleton<
            IValidateOptions<FileRelocationOptions>,
            FileRelocationOptionsValidator>();
        serviceCollection.AddOptions<FileRelocationOptions>()
            .Bind(configuration.GetSection(FileRelocationOptions.SectionName))
            .ValidateOnStart();
        serviceCollection.AddSingleton<FileRelocationOptions>(provider =>
            provider.GetRequiredService<IOptions<FileRelocationOptions>>().Value);
        serviceCollection.AddScoped<FileAccessPolicy>();
        serviceCollection.AddScoped<FileCreationService>();
        serviceCollection.AddScoped<FileRelocationService>();
        serviceCollection.AddScoped<FileStorageVersionService>();
        serviceCollection.AddScoped<FolderAccessPolicy>();
        serviceCollection.AddScoped<FolderAccessService>();
        serviceCollection.AddScoped<FolderCreationService>();
        serviceCollection.AddScoped<IFolderHierarchyLock, PostgreSqlFolderHierarchyLock>();
        serviceCollection.AddPostgreSqlEfRepository<
            FileManagerWriteDbContext,
            FileManagerReadDbContext>(
                configuration);
        serviceCollection.AddS3FileStorage(configuration.GetRequiredSection(nameof(FileManager)));
        serviceCollection.AddScheduling(configuration);

        return serviceCollection;
    }
}
