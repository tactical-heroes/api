using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
        serviceCollection.Configure<IdentityCleanupOptions>(
            configuration.GetSection(IdentityCleanupOptions.SectionName));

        var cleanupOptions = configuration
            .GetSection(IdentityCleanupOptions.SectionName)
            .Get<IdentityCleanupOptions>() ?? new IdentityCleanupOptions();

        serviceCollection.AddQuartz(options =>
        {
            options.AddJob<PruneUnconfirmedUsersJob>(job =>
                job.WithIdentity(PruneUnconfirmedUsersJob.Key));

            options.AddTrigger(trigger => trigger
                .ForJob(PruneUnconfirmedUsersJob.Key)
                .WithIdentity($"{nameof(PruneUnconfirmedUsersJob)}Trigger")
                .WithCronSchedule(cleanupOptions.UnconfirmedUsersCronSchedule));
        });

        serviceCollection.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

        return serviceCollection;
    }
}
