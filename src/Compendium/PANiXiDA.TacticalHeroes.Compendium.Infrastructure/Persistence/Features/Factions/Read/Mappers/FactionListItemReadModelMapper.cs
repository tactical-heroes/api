using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetList;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Factions.Read.DbModels;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Factions.Read.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal sealed partial class FactionListItemReadModelMapper
    : IReadModelMapper<Guid, FactionReadDbModel, FactionListItemReadModel>
{
    private static partial FactionListItemReadModel ToReadModel(
        FactionReadDbModel faction);

    public static partial IQueryable<FactionListItemReadModel> ProjectTo(
        IQueryable<FactionReadDbModel> query);
}
