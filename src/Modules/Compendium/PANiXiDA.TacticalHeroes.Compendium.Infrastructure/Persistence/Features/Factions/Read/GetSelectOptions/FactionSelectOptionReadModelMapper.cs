using PANiXiDA.TacticalHeroes.Modules.Compendium.Application.Factions.GetSelectOptions;
using PANiXiDA.TacticalHeroes.Modules.Compendium.Infrastructure.Persistence.Features.Factions.Read.DbModels;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Compendium.Infrastructure.Persistence.Features.Factions.Read.GetSelectOptions;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal sealed partial class FactionSelectOptionReadModelMapper
    : IReadModelMapper<Guid, FactionReadDbModel, FactionSelectOptionReadModel>
{
    public static partial IQueryable<FactionSelectOptionReadModel> ProjectTo(
        IQueryable<FactionReadDbModel> query);
}
