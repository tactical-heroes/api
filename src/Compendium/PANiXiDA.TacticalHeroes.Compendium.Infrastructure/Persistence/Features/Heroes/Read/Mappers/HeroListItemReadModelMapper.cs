using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.GetList;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Factions.Read.DbModels;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Heroes.Read.DbModels;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Heroes.Read.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal sealed partial class HeroListItemReadModelMapper
    : IReadModelMapper<Guid, HeroReadDbModel, HeroListItemReadModel>
{
    [MapProperty(
        $"{nameof(HeroReadDbModel.Faction)}.{nameof(FactionReadDbModel.Name)}",
        nameof(HeroListItemReadModel.FactionName))]
    private static partial HeroListItemReadModel ToReadModel(
        HeroReadDbModel hero);

    public static partial IQueryable<HeroListItemReadModel> ProjectTo(
        IQueryable<HeroReadDbModel> query);
}
