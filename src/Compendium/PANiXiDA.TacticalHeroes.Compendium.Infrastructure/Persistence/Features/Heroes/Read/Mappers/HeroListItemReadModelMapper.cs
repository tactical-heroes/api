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
            Id: hero.Id,
            Name: hero.Name,
            FactionId: hero.FactionId,
            FactionName: hero.Faction == null ? string.Empty : hero.Faction.Name));
    }
}
