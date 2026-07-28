using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetDetails;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Factions.Read.DbModels;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Factions.Read.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal sealed partial class FactionDetailsReadModelMapper
    : IReadModelMapper<Guid, FactionReadDbModel, FactionDetailsReadModel>
{
    private static partial FactionDetailsReadModel ToReadModel(
        FactionReadDbModel faction);

    public static partial IQueryable<FactionDetailsReadModel> ProjectTo(
        IQueryable<FactionReadDbModel> query);
}
