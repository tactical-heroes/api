using PANiXiDA.TacticalHeroes.Compendium.Application.Units.GetList;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Units.Read.DbModels;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Units.Read.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal sealed partial class UnitListItemReadModelMapper
    : IReadModelMapper<Guid, UnitReadDbModel, UnitListItemReadModel>
{
    [MapProperty(
        nameof(UnitReadDbModel.StatsAttack),
        nameof(UnitListItemReadModel.Attack))]
    [MapProperty(
        nameof(UnitReadDbModel.StatsDefense),
        nameof(UnitListItemReadModel.Defense))]
    [MapProperty(
        nameof(UnitReadDbModel.StatsHealth),
        nameof(UnitListItemReadModel.Health))]
    [MapProperty(
        nameof(UnitReadDbModel.StatsMinimumDamage),
        nameof(UnitListItemReadModel.MinimumDamage))]
    [MapProperty(
        nameof(UnitReadDbModel.StatsMaximumDamage),
        nameof(UnitListItemReadModel.MaximumDamage))]
    [MapProperty(
        nameof(UnitReadDbModel.StatsInitiative),
        nameof(UnitListItemReadModel.Initiative))]
    [MapProperty(
        nameof(UnitReadDbModel.StatsSpeed),
        nameof(UnitListItemReadModel.Speed))]
    [MapProperty(
        nameof(UnitReadDbModel.StatsShots),
        nameof(UnitListItemReadModel.Shots))]
    [MapProperty(
        nameof(UnitReadDbModel.StatsRangedAttackRange),
        nameof(UnitListItemReadModel.RangedAttackRange))]
    private static partial UnitListItemReadModel ToReadModel(
        UnitReadDbModel unit);

    public static partial IQueryable<UnitListItemReadModel> ProjectTo(
        IQueryable<UnitReadDbModel> query);
}
