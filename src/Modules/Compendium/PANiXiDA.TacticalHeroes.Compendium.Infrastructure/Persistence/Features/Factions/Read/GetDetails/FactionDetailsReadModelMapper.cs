using PANiXiDA.TacticalHeroes.Modules.Compendium.Application.Factions.GetDetails;
using PANiXiDA.TacticalHeroes.Modules.Compendium.Infrastructure.Persistence.Features.Factions.Read.DbModels;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Compendium.Infrastructure.Persistence.Features.Factions.Read.GetDetails;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal sealed partial class FactionDetailsReadModelMapper
    : IReadModelMapper<Guid, FactionReadDbModel, FactionDetailsReadModel>
{
    public static partial IQueryable<FactionDetailsReadModel> ProjectTo(
        IQueryable<FactionReadDbModel> query);

    private static partial FactionDetailsReadModel ToReadModel(
        FactionReadDbModel faction);
}
