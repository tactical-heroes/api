using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.GetList;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Heroes.Read.DbModels;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Heroes.Read.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal sealed partial class HeroListItemReadModelMapper
    : IReadModelMapper<Guid, HeroReadDbModel, HeroListItemReadModel>
{
    [MapPropertyFromSource(
        nameof(HeroListItemReadModel.FactionName),
        Use = nameof(MapFactionName))]
    private static partial HeroListItemReadModel ToReadModel(
        HeroReadDbModel hero);

    public static partial IQueryable<HeroListItemReadModel> ProjectTo(
        IQueryable<HeroReadDbModel> query);

    private static string MapFactionName(HeroReadDbModel hero)
        => hero.Faction != null ? hero.Faction.Name : string.Empty;
}
