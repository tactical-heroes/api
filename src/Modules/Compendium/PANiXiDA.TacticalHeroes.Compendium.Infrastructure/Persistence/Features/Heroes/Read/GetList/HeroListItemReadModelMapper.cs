using PANiXiDA.TacticalHeroes.Modules.Compendium.Application.Heroes.GetList;
using PANiXiDA.TacticalHeroes.Modules.Compendium.Infrastructure.Persistence.Features.Factions.Read.DbModels;
using PANiXiDA.TacticalHeroes.Modules.Compendium.Infrastructure.Persistence.Features.Heroes.Read.DbModels;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Compendium.Infrastructure.Persistence.Features.Heroes.Read.GetList;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal sealed partial class HeroListItemReadModelMapper
    : IReadModelMapper<Guid, HeroReadDbModel, HeroListItemReadModel>
{
    public static partial IQueryable<HeroListItemReadModel> ProjectTo(
        IQueryable<HeroReadDbModel> query);

    [MapProperty(
        $"{nameof(HeroReadDbModel.Faction)}.{nameof(FactionReadDbModel.Name)}",
        nameof(HeroListItemReadModel.FactionName),
        SuppressNullMismatchDiagnostic = true)]
    private static partial HeroListItemReadModel ToReadModel(
        HeroReadDbModel hero);
}
