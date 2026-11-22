using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Builds.Application.Builds.Abstractions;
using PANiXiDA.TacticalHeroes.Builds.Application.Builds.GetInternalBuild;
using PANiXiDA.TacticalHeroes.Builds.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Builds.Infrastructure.Persistence.Features.Builds.Read.DbModels;
using PANiXiDA.TacticalHeroes.Builds.Infrastructure.Persistence.Features.Builds.Read.GetInternalBuild;

namespace PANiXiDA.TacticalHeroes.Builds.Infrastructure.Persistence.Features.Builds.Read;

public sealed class BuildsReadRepository(BuildsReadDbContext dbContext)
    :
        EfReadRepository<BuildsReadDbContext, Guid, BuildReadDbModel>(dbContext),
        IBuildsReadRepository
{
    public Task<BuildReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var query = Query.Where(build => build.Id == id);
        return BuildReadModelMapper.ProjectTo(query).SingleOrDefaultAsync(cancellationToken);
    }
}
