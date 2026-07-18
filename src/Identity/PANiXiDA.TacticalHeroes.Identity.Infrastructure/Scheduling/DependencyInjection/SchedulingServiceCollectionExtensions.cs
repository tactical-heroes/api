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
        var cleanupSection = configuration.GetSection(IdentityCleanupOptions.SectionName);
        var cleanupOptions = cleanupSection.Get<IdentityCleanupOptions>() ??
            new IdentityCleanupOptions();

        serviceCollection.AddSingleton<
            IValidateOptions<IdentityCleanupOptions>,
            IdentityCleanupOptionsValidator>();
        serviceCollection
            .AddOptions<IdentityCleanupOptions>()
            .Bind(cleanupSection)
            .ValidateOnStart();

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
