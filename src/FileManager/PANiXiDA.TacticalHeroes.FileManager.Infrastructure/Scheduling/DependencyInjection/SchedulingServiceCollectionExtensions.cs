using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Scheduling.Options.FileUploadReconciliation;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Scheduling.Reconciliation;

using Quartz;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Scheduling.DependencyInjection;

internal static class SchedulingServiceCollectionExtensions
{
    internal static IServiceCollection AddScheduling(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var section = configuration.GetSection(FileUploadReconciliationOptions.SectionName);
        var reconciliationOptions = section.Get<FileUploadReconciliationOptions>() ?? new();
        services.AddSingleton<
            IValidateOptions<FileUploadReconciliationOptions>,
            FileUploadReconciliationOptionsValidator>();
        services.AddOptions<FileUploadReconciliationOptions>()
            .Bind(section)
            .ValidateOnStart();

        services.AddQuartz(options =>
        {
            if (!reconciliationOptions.Enabled)
            {
                return;
            }

            options.AddJob<ReconcilePersonalFileUploadsJob>(job =>
                job.WithIdentity(ReconcilePersonalFileUploadsJob.Key));
            options.AddTrigger(trigger => trigger
                .ForJob(ReconcilePersonalFileUploadsJob.Key)
                .WithIdentity($"{nameof(ReconcilePersonalFileUploadsJob)}Trigger")
                .WithCronSchedule(reconciliationOptions.CronSchedule));
        });
        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

        return services;
    }
}
