using PANiXiDA.TacticalHeroes.Builds.Application.Builds.Abstractions;
using PANiXiDA.TacticalHeroes.Builds.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Builds.Infrastructure.Persistence.Features.Builds.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Builds.Infrastructure.Persistence.Features.Builds.Read;

public sealed class BuildsReadRepository(BuildsReadDbContext dbContext)
    :
        EfReadRepository<BuildsReadDbContext, Guid, BuildReadDbModel>(dbContext),
        IBuildsReadRepository;
