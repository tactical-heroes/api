namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.EmailRequests;

internal sealed class EmailRequestCooldown
{
    public string NormalizedEmail { get; set; } = string.Empty;
    public EmailRequestPurpose Purpose { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}
