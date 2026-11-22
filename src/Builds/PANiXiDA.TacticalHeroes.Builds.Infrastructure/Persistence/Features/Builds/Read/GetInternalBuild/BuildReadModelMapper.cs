using PANiXiDA.TacticalHeroes.Builds.Application.Builds.GetInternalBuild;
using PANiXiDA.TacticalHeroes.Builds.Infrastructure.Persistence.Features.Builds.Read.DbModels;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Builds.Infrastructure.Persistence.Features.Builds.Read.GetInternalBuild;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal sealed partial class BuildReadModelMapper
    : IReadModelMapper<Guid, BuildReadDbModel, BuildReadModel>
{
    public static partial IQueryable<BuildReadModel> ProjectTo(IQueryable<BuildReadDbModel> query);

    [MapProperty(nameof(BuildReadDbModel.Heroes), nameof(BuildReadModel.HeroIds))]
    [MapProperty(nameof(BuildReadDbModel.Units), nameof(BuildReadModel.UnitIds))]
    private static partial BuildReadModel ToReadModel(BuildReadDbModel build);

    private static Guid ToHeroId(BuildHeroReadDbModel hero) => hero.HeroId;

    private static Guid ToUnitId(BuildUnitReadDbModel unit) => unit.UnitId;
}
