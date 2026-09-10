using PANiXiDA.TacticalHeroes.Compendium.Application.Units.GetList;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Units.Read.DbModels;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Units.Read.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal sealed partial class UnitListItemReadModelMapper
    : IReadModelMapper<Guid, UnitReadDbModel, UnitListItemReadModel>
{
    [MapPropertyFromSource(
        nameof(UnitListItemReadModel.FactionName),
        Use = nameof(MapFactionName))]
    private static partial UnitListItemReadModel ToReadModel(
        UnitReadDbModel unit);

    public static partial IQueryable<UnitListItemReadModel> ProjectTo(
        IQueryable<UnitReadDbModel> query);

    private static string MapFactionName(UnitReadDbModel unit)
        => unit.Faction != null ? unit.Faction.Name : string.Empty;
}
