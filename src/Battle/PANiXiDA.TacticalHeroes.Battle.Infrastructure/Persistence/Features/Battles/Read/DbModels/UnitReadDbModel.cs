namespace PANiXiDA.TacticalHeroes.Battle.Infrastructure.Persistence.Features.Battles.Read.DbModels;

public sealed class UnitReadDbModel : ReadDbModel<Guid>
{
    public Guid BattleId { get; set; }
    public int StatsAttack { get; set; }
    public int StatsDefense { get; set; }
    public int StatsMaxHealth { get; set; }
    public int DamageRangeMinimum { get; set; }
    public int DamageRangeMaximum { get; set; }
    public int StackStateCount { get; set; }
    public int StackStateCurrentUnitHealth { get; set; }
}
