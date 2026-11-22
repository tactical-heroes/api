using PANiXiDA.TacticalHeroes.Modules.Builds.Application.Builds.GetInternalBuild;
using PANiXiDA.TacticalHeroes.Modules.Builds.Infrastructure.Persistence.Features.Builds.Read.DbModels;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Builds.Infrastructure.Persistence.Features.Builds.Read.GetInternalBuild;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal sealed partial class BuildReadModelMapper
    : IReadModelMapper<Guid, BuildReadDbModel, BuildReadModel>
{
    public static partial IQueryable<BuildReadModel> ProjectTo(IQueryable<BuildReadDbModel> query);

    private static partial BuildReadModel ToReadModel(BuildReadDbModel build);
}
