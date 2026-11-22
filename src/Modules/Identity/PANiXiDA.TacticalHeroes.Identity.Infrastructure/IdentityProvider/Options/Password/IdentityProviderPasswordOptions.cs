namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options.Password;

public sealed class IdentityProviderPasswordOptions
{
    public int RequiredLength { get; init; } = 8;

    public int RequiredUniqueChars { get; init; } = 1;

    public bool RequireDigit { get; init; } = true;

    public bool RequireLowercase { get; init; } = true;

    public bool RequireNonAlphanumeric { get; init; } = true;

    public bool RequireUppercase { get; init; } = true;
}
