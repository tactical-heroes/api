namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options;

internal sealed class IdentityProviderOptions
{
    public const string SectionName = "Identity:Provider";

    public TimeSpan AccessTokenLifetime { get; init; } = TimeSpan.FromMinutes(15);

    public TimeSpan RefreshTokenLifetime { get; init; } = TimeSpan.FromDays(30);

    public List<IdentityProviderClientOptions> Clients { get; init; } =
    [
        new()
    ];
}
