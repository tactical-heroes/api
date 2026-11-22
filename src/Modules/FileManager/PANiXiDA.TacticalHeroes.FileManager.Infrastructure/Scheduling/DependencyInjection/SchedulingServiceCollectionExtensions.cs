using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Common.Options.FileRelocation;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Scheduling.Options.FileRelocation;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Scheduling.Options.FileUploadReconciliation;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Scheduling.DependencyInjection;

internal static class SchedulingServiceCollectionExtensions
{
    internal static IServiceCollection AddScheduling(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<
            IValidateOptions<FileRelocationOptions>,
            FileRelocationOptionsValidator>();
        services.AddOptions<FileRelocationOptions>()
            .Bind(configuration.GetSection(FileRelocationOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<FileRelocationOptions>(provider =>
            provider.GetRequiredService<IOptions<FileRelocationOptions>>().Value);

        var section = configuration.GetSection(FileUploadReconciliationOptions.SectionName);
        services.AddSingleton<
            IValidateOptions<FileUploadReconciliationOptions>,
            FileUploadReconciliationOptionsValidator>();
        services.AddOptions<FileUploadReconciliationOptions>()
            .Bind(section)
            .ValidateOnStart();

        return services;
    }
}
