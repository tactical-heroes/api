using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Options.FileUploadReconciliation;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Scheduling.DependencyInjection;

internal static class SchedulingServiceCollectionExtensions
{
    internal static IServiceCollection AddScheduling(
        this IServiceCollection services,
        IConfiguration configuration)
    {
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
