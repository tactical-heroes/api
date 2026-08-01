using PANiXiDA.TacticalHeroes.Compendium.Application.Units.GetList;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Factions.Read.DbModels;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Units.Read.DbModels;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Units.Read.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal sealed partial class UnitListItemReadModelMapper
    : IReadModelMapper<Guid, UnitReadDbModel, UnitListItemReadModel>
{
    [MapProperty(
        $"{nameof(UnitReadDbModel.Faction)}.{nameof(FactionReadDbModel.Name)}",
        nameof(UnitListItemReadModel.FactionName))]
    private static partial UnitListItemReadModel ToReadModel(
        UnitReadDbModel unit);

    public static partial IQueryable<UnitListItemReadModel> ProjectTo(
        IQueryable<UnitReadDbModel> query);
}
