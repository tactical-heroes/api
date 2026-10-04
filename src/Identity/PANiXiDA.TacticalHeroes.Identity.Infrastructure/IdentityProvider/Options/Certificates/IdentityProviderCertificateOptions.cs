namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options.Certificates;

public sealed class IdentityProviderCertificateOptions
{
    public string PfxBase64 { get; init; } = string.Empty;

    public string? Password { get; init; }
}
