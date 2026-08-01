using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Factions.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Heroes.Read.DbModels;

public sealed class HeroReadDbModel : AuditableReadDbModel<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int StatsAttack { get; set; }
    public int StatsDefense { get; set; }
    public int StatsMinimumDamage { get; set; }
    public int StatsMaximumDamage { get; set; }
    public double StatsInitiative { get; set; }
    public int Morale { get; set; }
    public int Luck { get; set; }
    public Guid FactionId { get; set; }

    public FactionReadDbModel? Faction { get; set; }
}
