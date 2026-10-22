using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options.EmailRequestCooldown;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Scheduling.Cleanup;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Scheduling.Options.IdentityCleanup;

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
        var emailCooldownOptions = configuration.GetSection(EmailRequestCooldownOptions.SectionName)
            .Get<EmailRequestCooldownOptions>()
            ?? new EmailRequestCooldownOptions();

        serviceCollection.AddSingleton<
            IValidateOptions<IdentityCleanupOptions>,
            IdentityCleanupOptionsValidator>();
        serviceCollection
            .AddOptions<IdentityCleanupOptions>()
            .Bind(cleanupSection)
            .ValidateOnStart();

        serviceCollection.AddQuartz(options =>
        {
            options.AddJob<PruneEmailRequestCooldownsJob>(job => job.WithIdentity(PruneEmailRequestCooldownsJob.Key));
            options.AddTrigger(trigger => trigger
                .ForJob(PruneEmailRequestCooldownsJob.Key)
                .WithIdentity($"{nameof(PruneEmailRequestCooldownsJob)}Trigger")
                .WithCronSchedule(emailCooldownOptions.CleanupCronSchedule));

            if (!cleanupOptions.PruneUnconfirmedUsersEnabled)
            {
                return;
            }

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
