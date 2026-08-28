using PANiXiDA.TacticalHeroes.Compendium.Application.Units.GetList;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Units.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Units.Read.Mappers;

internal sealed class UnitListItemReadModelMapper
    : IReadModelMapper<Guid, UnitReadDbModel, UnitListItemReadModel>
{
    public static IQueryable<UnitListItemReadModel> ProjectTo(
        IQueryable<UnitReadDbModel> query)
    {
        return query.Select(unit => new UnitListItemReadModel(
            unit.Id,
            unit.Name,
            unit.FactionId,
            unit.Faction == null ? string.Empty : unit.Faction.Name));
    }
}
