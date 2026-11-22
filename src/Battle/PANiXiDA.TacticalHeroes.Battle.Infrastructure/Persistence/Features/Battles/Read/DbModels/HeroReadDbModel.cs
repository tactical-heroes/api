namespace PANiXiDA.TacticalHeroes.Battle.Infrastructure.Persistence.Features.Battles.Read.DbModels;

public sealed class HeroReadDbModel : ReadDbModel<Guid>
{
    public Guid BattleId { get; set; }
    public Guid OwnerPlayerId { get; set; }
    public Guid DefinitionId { get; set; }
    public Guid FactionId { get; set; }
    public int StatsAttack { get; set; }
    public int StatsDefense { get; set; }
    public int StatsMinimumDamage { get; set; }
    public int StatsMaximumDamage { get; set; }
    public double StatsInitiative { get; set; }
    public int StatsMorale { get; set; }
    public int StatsLuck { get; set; }

    public BattleReadDbModel? Battle { get; set; }
}
