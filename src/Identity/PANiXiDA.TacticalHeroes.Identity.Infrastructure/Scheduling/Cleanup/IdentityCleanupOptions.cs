namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Scheduling.Cleanup;

internal sealed class IdentityCleanupOptions
{
    public const string SectionName = "Identity:Cleanup";

    public TimeSpan UnconfirmedUserRetention { get; init; } = TimeSpan.FromDays(7);

    public string UnconfirmedUsersCronSchedule { get; init; } = "0 0 * * * ?";

    public string ExpiredConfirmationTokensCronSchedule { get; init; } = "0 5 * * * ?";

    public string ExpiredPasswordResetTokensCronSchedule { get; init; } = "0 10 * * * ?";
}
