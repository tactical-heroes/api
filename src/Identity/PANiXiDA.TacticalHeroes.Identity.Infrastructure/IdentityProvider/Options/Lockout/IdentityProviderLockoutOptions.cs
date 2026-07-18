namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options.Lockout;

public sealed class IdentityProviderLockoutOptions
{
    public int MaxFailedAccessAttempts { get; init; } = 5;

    public TimeSpan DefaultLockoutTimeSpan { get; init; } = TimeSpan.FromMinutes(minutes: 5);
}
