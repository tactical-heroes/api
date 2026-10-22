namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Common.Options.AuthRateLimit;

internal sealed class AuthEndpointRateLimitOptions
{
    public int PermitLimit { get; set; }
    public TimeSpan Window { get; set; } = TimeSpan.FromMinutes(1);
}
