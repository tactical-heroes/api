namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options.EmailRequestCooldown;

public sealed class EmailRequestCooldownOptions
{
    public const string SectionName = "Identity:EmailRequestCooldown";

    public TimeSpan Confirmation { get; set; } = TimeSpan.FromMinutes(1);
    public TimeSpan PasswordReset { get; set; } = TimeSpan.FromMinutes(1);
    public string CleanupCronSchedule { get; set; } = "0 0 * * * ?";
}
