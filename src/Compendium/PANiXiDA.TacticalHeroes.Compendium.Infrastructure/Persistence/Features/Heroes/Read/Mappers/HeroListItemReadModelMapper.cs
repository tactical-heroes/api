using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.GetList;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Heroes.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Heroes.Read.Mappers;

internal sealed class HeroListItemReadModelMapper
    : IReadModelMapper<Guid, HeroReadDbModel, HeroListItemReadModel>
{
    public static IQueryable<HeroListItemReadModel> ProjectTo(
        IQueryable<HeroReadDbModel> query)
    {
        return query.Select(hero => new HeroListItemReadModel(
            hero.Id,
            hero.Name,
            hero.FactionId,
            hero.Faction == null ? string.Empty : hero.Faction.Name));
    }
}
