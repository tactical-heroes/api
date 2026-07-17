namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options;

public sealed class IdentityProviderOptions
{
    public const string SectionName = "Identity:Provider";

    public Uri? Issuer { get; init; }

    public string Audience { get; init; } = string.Empty;

    public TimeSpan AccessTokenLifetime { get; init; } = TimeSpan.FromMinutes(15);

    public TimeSpan RefreshTokenLifetime { get; init; } = TimeSpan.FromDays(30);

    public TimeSpan AuthorizationCodeLifetime { get; init; } = TimeSpan.FromMinutes(5);

    public TimeSpan IdentityTokenLifetime { get; init; } = TimeSpan.FromMinutes(5);

    public TimeSpan EmailConfirmationTokenLifetime { get; init; } = TimeSpan.FromHours(24);

    public TimeSpan PasswordResetTokenLifetime { get; init; } = TimeSpan.FromHours(1);

    public IdentityProviderUserOptions User { get; init; } = new();

    public IdentityProviderPasswordOptions Password { get; init; } = new();

    public IdentityProviderLockoutOptions Lockout { get; init; } = new();

    public IdentityProviderTokenProviderOptions TokenProviders { get; init; } = new();

    public List<IdentityProviderClientOptions> Clients { get; init; } =
    [
        new()
    ];
}
