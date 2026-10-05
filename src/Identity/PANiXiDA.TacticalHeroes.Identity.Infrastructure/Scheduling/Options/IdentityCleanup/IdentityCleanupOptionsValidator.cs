using Microsoft.Extensions.Options;

using Quartz;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Scheduling.Options.IdentityCleanup;

internal sealed class IdentityCleanupOptionsValidator
    : IValidateOptions<IdentityCleanupOptions>
{
    public ValidateOptionsResult Validate(
        string? name,
        IdentityCleanupOptions options)
    {
        List<string> failures = [];

        if (!options.PruneUnconfirmedUsersEnabled)
        {
            return ValidateOptionsResult.Success;
        }

        if (options.UnconfirmedUserRetention <= TimeSpan.Zero)
        {
            failures.Add(
                item: $"{IdentityCleanupOptions.SectionName}:UnconfirmedUserRetention must be positive.");
        }

        if (string.IsNullOrWhiteSpace(options.UnconfirmedUsersCronSchedule) ||
            !CronExpression.TryParse(cronExpression: options.UnconfirmedUsersCronSchedule, result: out _))
        {
            failures.Add(
                item: $"{IdentityCleanupOptions.SectionName}:UnconfirmedUsersCronSchedule must be a valid Quartz cron expression.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures: failures);
    }
}
