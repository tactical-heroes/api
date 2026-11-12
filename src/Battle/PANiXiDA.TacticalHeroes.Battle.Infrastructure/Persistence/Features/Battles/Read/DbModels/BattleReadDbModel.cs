namespace PANiXiDA.TacticalHeroes.Battle.Infrastructure.Persistence.Features.Battles.Read.DbModels;

public sealed class BattleReadDbModel : AuditableReadDbModel<Guid>
{
    public Guid? GameId { get; set; }
    public Guid? FirstPlayerId { get; set; }
    public Guid? SecondPlayerId { get; set; }
    public string Status { get; set; } = string.Empty;
    public ICollection<HeroReadDbModel> Heroes { get; set; } = [];
    public ICollection<UnitReadDbModel> Units { get; set; } = [];
}
