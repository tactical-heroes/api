using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetDetails;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Factions.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Factions.Read.Mappers;

internal sealed class FactionDetailsReadModelMapper
    : IReadModelMapper<Guid, FactionReadDbModel, FactionDetailsReadModel>
{
    public static IQueryable<FactionDetailsReadModel> ProjectTo(
        IQueryable<FactionReadDbModel> query)
    {
        return query.Select(faction => new FactionDetailsReadModel(
            faction.Id,
            faction.Name,
            faction.Description));
    }
}
