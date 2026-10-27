using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchSearches.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.Matches.Read.DbModels;

public sealed class MatchReadDbModel : AuditableReadDbModel<Guid>
{
    public string Mode { get; set; } = string.Empty;
    public DateTimeOffset MatchedAt { get; set; }
    public DateTimeOffset CreationDeadline { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? GameId { get; set; }

    public ICollection<MatchParticipantReadDbModel> Participants { get; set; } = [];
    public ICollection<MatchSearchReadDbModel> MatchSearches { get; set; } = [];
}
