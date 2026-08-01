using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.GetDetails;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Heroes.Read.DbModels;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Heroes.Read.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal sealed partial class HeroDetailsReadModelMapper
    : IReadModelMapper<Guid, HeroReadDbModel, HeroDetailsReadModel>
{
    [MapProperty(
        nameof(HeroReadDbModel.StatsAttack),
        nameof(HeroDetailsReadModel.Attack))]
    [MapProperty(
        nameof(HeroReadDbModel.StatsDefense),
        nameof(HeroDetailsReadModel.Defense))]
    [MapProperty(
        nameof(HeroReadDbModel.StatsMinimumDamage),
        nameof(HeroDetailsReadModel.MinimumDamage))]
    [MapProperty(
        nameof(HeroReadDbModel.StatsMaximumDamage),
        nameof(HeroDetailsReadModel.MaximumDamage))]
    [MapProperty(
        nameof(HeroReadDbModel.StatsInitiative),
        nameof(HeroDetailsReadModel.Initiative))]
    private static partial HeroDetailsReadModel ToReadModel(
        HeroReadDbModel hero);

    public static partial IQueryable<HeroDetailsReadModel> ProjectTo(
        IQueryable<HeroReadDbModel> query);
}
