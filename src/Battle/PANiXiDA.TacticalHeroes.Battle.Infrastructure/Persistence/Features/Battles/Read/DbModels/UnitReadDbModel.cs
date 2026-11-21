namespace PANiXiDA.TacticalHeroes.Battle.Infrastructure.Persistence.Features.Battles.Read.DbModels;

public sealed class UnitReadDbModel : ReadDbModel<Guid>
{
    public Guid BattleId { get; set; }
    public Guid? OwnerPlayerId { get; set; }
    public Guid? DefinitionId { get; set; }
    public Guid? FactionId { get; set; }
    public double? TraitsInitiative { get; set; }
    public int? TraitsSpeed { get; set; }
    public int? TraitsMorale { get; set; }
    public int? TraitsLuck { get; set; }
    public int? TraitsShots { get; set; }
    public int? TraitsRangedAttackRange { get; set; }
    public int StatsAttack { get; set; }
    public int StatsDefense { get; set; }
    public int StatsMaxHealth { get; set; }
    public int DamageRangeMinimum { get; set; }
    public int DamageRangeMaximum { get; set; }
    public int StackStateCount { get; set; }
    public int StackStateCurrentUnitHealth { get; set; }

    public BattleReadDbModel? Battle { get; set; }
}
