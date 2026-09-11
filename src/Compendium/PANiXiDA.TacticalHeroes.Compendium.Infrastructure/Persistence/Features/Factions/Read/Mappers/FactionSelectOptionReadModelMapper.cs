using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetSelectOptions;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Factions.Read.DbModels;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Factions.Read.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal sealed partial class FactionSelectOptionReadModelMapper
    : IReadModelMapper<Guid, FactionReadDbModel, FactionSelectOptionReadModel>
{
    private static partial FactionSelectOptionReadModel ToReadModel(FactionReadDbModel faction);

    public static partial IQueryable<FactionSelectOptionReadModel> ProjectTo(
        IQueryable<FactionReadDbModel> query);
}
