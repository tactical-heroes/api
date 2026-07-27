using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetList;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Factions.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Factions.Read.Mappers;

internal sealed class FactionListItemReadModelMapper
    : IReadModelMapper<Guid, FactionReadDbModel, FactionListItemReadModel>
{
    public static IQueryable<FactionListItemReadModel> ProjectTo(
        IQueryable<FactionReadDbModel> query)
    {
        return query.Select(faction => new FactionListItemReadModel(
            faction.Id,
            faction.Name,
            faction.Description));
    }
}
