namespace PANiXiDA.TacticalHeroes.Games.Infrastructure.Persistence.Features.Games.Read.DbModels;

public sealed class GameReadDbModel : AuditableReadDbModel<Guid>
{
    public string Mode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid? BattleId { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }
    public bool? ResultIsDraw { get; set; }
    public Guid? ResultWinnerPlayerId { get; set; }
    public ICollection<GameParticipantReadDbModel> Participants { get; set; } = [];
}
