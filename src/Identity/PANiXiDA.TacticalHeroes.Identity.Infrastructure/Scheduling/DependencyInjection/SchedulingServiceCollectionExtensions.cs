using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
        serviceCollection.TryAddSingleton(TimeProvider.System);
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

            options.AddJob<PruneExpiredUserConfirmationTokensJob>(job =>
                job.WithIdentity(PruneExpiredUserConfirmationTokensJob.Key));

            options.AddTrigger(trigger => trigger
                .ForJob(PruneExpiredUserConfirmationTokensJob.Key)
                .WithIdentity($"{nameof(PruneExpiredUserConfirmationTokensJob)}Trigger")
                .WithCronSchedule(cleanupOptions.ExpiredConfirmationTokensCronSchedule));

            options.AddJob<PruneExpiredUserPasswordResetTokensJob>(job =>
                job.WithIdentity(PruneExpiredUserPasswordResetTokensJob.Key));

            options.AddTrigger(trigger => trigger
                .ForJob(PruneExpiredUserPasswordResetTokensJob.Key)
                .WithIdentity($"{nameof(PruneExpiredUserPasswordResetTokensJob)}Trigger")
                .WithCronSchedule(cleanupOptions.ExpiredPasswordResetTokensCronSchedule));
        });

        serviceCollection.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

        return serviceCollection;
    }
}
