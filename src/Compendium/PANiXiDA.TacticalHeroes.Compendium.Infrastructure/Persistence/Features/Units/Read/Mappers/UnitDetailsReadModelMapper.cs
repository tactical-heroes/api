using PANiXiDA.TacticalHeroes.Compendium.Application.Units.GetDetails;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Units.Read.DbModels;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Units.Read.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal sealed partial class UnitDetailsReadModelMapper
    : IReadModelMapper<Guid, UnitReadDbModel, UnitDetailsReadModel>
{
    [MapProperty(
        nameof(UnitReadDbModel.StatsAttack),
        nameof(UnitDetailsReadModel.Attack))]
    [MapProperty(
        nameof(UnitReadDbModel.StatsDefense),
        nameof(UnitDetailsReadModel.Defense))]
    [MapProperty(
        nameof(UnitReadDbModel.StatsHealth),
        nameof(UnitDetailsReadModel.Health))]
    [MapProperty(
        nameof(UnitReadDbModel.StatsMinimumDamage),
        nameof(UnitDetailsReadModel.MinimumDamage))]
    [MapProperty(
        nameof(UnitReadDbModel.StatsMaximumDamage),
        nameof(UnitDetailsReadModel.MaximumDamage))]
    [MapProperty(
        nameof(UnitReadDbModel.StatsInitiative),
        nameof(UnitDetailsReadModel.Initiative))]
    [MapProperty(
        nameof(UnitReadDbModel.StatsSpeed),
        nameof(UnitDetailsReadModel.Speed))]
    [MapProperty(
        nameof(UnitReadDbModel.StatsShots),
        nameof(UnitDetailsReadModel.Shots))]
    [MapProperty(
        nameof(UnitReadDbModel.StatsRangedAttackRange),
        nameof(UnitDetailsReadModel.RangedAttackRange))]
    private static partial UnitDetailsReadModel ToReadModel(
        UnitReadDbModel unit);

    public static partial IQueryable<UnitDetailsReadModel> ProjectTo(
        IQueryable<UnitReadDbModel> query);
}
