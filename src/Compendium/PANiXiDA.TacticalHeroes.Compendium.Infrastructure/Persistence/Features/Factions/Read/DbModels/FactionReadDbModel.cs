using System.ComponentModel.DataAnnotations;

using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Heroes.Read.DbModels;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Units.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Factions.Read.DbModels;

public sealed class FactionReadDbModel : AuditableReadDbModel<Guid>
{
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    public ICollection<HeroReadDbModel> Heroes { get; set; } = [];
    public ICollection<UnitReadDbModel> Units { get; set; } = [];
}
