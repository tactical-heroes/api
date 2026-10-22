using Microsoft.Extensions.Options;

using Quartz;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options.EmailRequestCooldown;

internal sealed class EmailRequestCooldownOptionsValidator : IValidateOptions<EmailRequestCooldownOptions>
{
    public ValidateOptionsResult Validate(string? name, EmailRequestCooldownOptions options)
    {
        var section = EmailRequestCooldownOptions.SectionName;
        List<string> failures = [];
        if (options.Confirmation <= TimeSpan.Zero || options.AccountRecovery <= TimeSpan.Zero)
        {
            failures.Add(
                $"{section} requires positive Confirmation and AccountRecovery intervals.");
        }

        if (string.IsNullOrWhiteSpace(options.CleanupCronSchedule)
            || !CronExpression.TryParse(options.CleanupCronSchedule, out _))
        {
            failures.Add(
                $"{section}:CleanupCronSchedule must be a valid Quartz cron expression.");
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}
