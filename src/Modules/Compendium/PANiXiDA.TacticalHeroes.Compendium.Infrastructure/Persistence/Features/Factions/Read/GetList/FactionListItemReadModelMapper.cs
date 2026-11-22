using PANiXiDA.TacticalHeroes.Modules.Compendium.Application.Factions.GetList;
using PANiXiDA.TacticalHeroes.Modules.Compendium.Infrastructure.Persistence.Features.Factions.Read.DbModels;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Compendium.Infrastructure.Persistence.Features.Factions.Read.GetList;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal sealed partial class FactionListItemReadModelMapper
    : IReadModelMapper<Guid, FactionReadDbModel, FactionListItemReadModel>
{
    public static partial IQueryable<FactionListItemReadModel> ProjectTo(
        IQueryable<FactionReadDbModel> query);

    private static partial FactionListItemReadModel ToReadModel(
        FactionReadDbModel faction);
}
