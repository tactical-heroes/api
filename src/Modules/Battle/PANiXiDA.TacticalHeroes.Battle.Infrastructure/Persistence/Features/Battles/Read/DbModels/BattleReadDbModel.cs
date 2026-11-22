namespace PANiXiDA.TacticalHeroes.Modules.Battle.Infrastructure.Persistence.Features.Battles.Read.DbModels;

public sealed class BattleReadDbModel : AuditableReadDbModel<Guid>
{
    public ICollection<UnitReadDbModel> Units { get; set; } = [];
}
