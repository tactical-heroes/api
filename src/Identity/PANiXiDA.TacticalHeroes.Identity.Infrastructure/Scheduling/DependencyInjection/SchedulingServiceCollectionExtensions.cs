using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Scheduling.Cleanup;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Scheduling.Options;

using Quartz;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Scheduling.DependencyInjection;

internal static class SchedulingServiceCollectionExtensions
{
    public static IServiceCollection AddScheduling(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        var cleanupSection = configuration.GetSection(key: IdentityCleanupOptions.SectionName);
        var cleanupOptions = cleanupSection.Get<IdentityCleanupOptions>() ??
            new IdentityCleanupOptions();

        serviceCollection.AddSingleton<
            IValidateOptions<IdentityCleanupOptions>,
            IdentityCleanupOptionsValidator>();
        serviceCollection
            .AddOptions<IdentityCleanupOptions>()
            .Bind(config: cleanupSection)
            .ValidateOnStart();

        serviceCollection.AddQuartz(configure: options =>
        {
            if (!cleanupOptions.PruneUnconfirmedUsersEnabled)
            {
                return;
            }

            options.AddJob<PruneUnconfirmedUsersJob>(configure: job =>
                job.WithIdentity(key: PruneUnconfirmedUsersJob.Key));

            options.AddTrigger(configure: trigger => trigger
                .ForJob(jobKey: PruneUnconfirmedUsersJob.Key)
                .WithIdentity(name: $"{nameof(PruneUnconfirmedUsersJob)}Trigger")
                .WithCronSchedule(cronExpression: cleanupOptions.UnconfirmedUsersCronSchedule));
        });

        serviceCollection.AddQuartzHostedService(configure: options => options.WaitForJobsToComplete = true);

        return serviceCollection;
    }
}
