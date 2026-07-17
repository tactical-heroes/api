namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options;

public sealed class IdentityProviderLockoutOptions
{
    public int MaxFailedAccessAttempts { get; init; } = 5;

    public TimeSpan DefaultLockoutTimeSpan { get; init; } = TimeSpan.FromMinutes(5);
}
