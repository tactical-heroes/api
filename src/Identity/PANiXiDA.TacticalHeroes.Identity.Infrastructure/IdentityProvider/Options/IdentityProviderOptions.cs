namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options;

public sealed class IdentityProviderOptions
{
    public const string SectionName = "Identity:Provider";

    public TimeSpan AccessTokenLifetime { get; init; } = TimeSpan.FromMinutes(15);

    public TimeSpan RefreshTokenLifetime { get; init; } = TimeSpan.FromDays(30);

    public TimeSpan EmailConfirmationTokenLifetime { get; init; } = TimeSpan.FromHours(24);

    public TimeSpan PasswordResetTokenLifetime { get; init; } = TimeSpan.FromHours(1);

    public IdentityProviderUserOptions User { get; init; } = new();

    public IdentityProviderPasswordOptions Password { get; init; } = new();

    public IdentityProviderTokenProviderOptions TokenProviders { get; init; } = new();

    public List<IdentityProviderClientOptions> Clients { get; init; } =
    [
        new()
    ];
}
