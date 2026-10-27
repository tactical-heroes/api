namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchSearches.Read.DbModels;

public sealed class MatchSearchReadDbModel : AuditableReadDbModel<Guid>
{
    public Guid PlayerId { get; set; }
    public Guid BuildId { get; set; }
    public int Rating { get; set; }
    public string Mode { get; set; } = string.Empty;
    public DateTimeOffset QueuedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public string Status { get; set; } = string.Empty;
}
