using Microsoft.Extensions.Options;

using Quartz;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Scheduling.Options;

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
                $"{IdentityCleanupOptions.SectionName}:UnconfirmedUserRetention must be positive.");
        }

        if (string.IsNullOrWhiteSpace(options.UnconfirmedUsersCronSchedule) ||
            !CronExpression.IsValidExpression(options.UnconfirmedUsersCronSchedule))
        {
            failures.Add(
                $"{IdentityCleanupOptions.SectionName}:UnconfirmedUsersCronSchedule must be a valid Quartz cron expression.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures: failures);
    }
}
