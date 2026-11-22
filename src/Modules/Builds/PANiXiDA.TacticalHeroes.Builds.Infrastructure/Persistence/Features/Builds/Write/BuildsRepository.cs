using PANiXiDA.TacticalHeroes.Builds.Domain.Builds;
using PANiXiDA.TacticalHeroes.Builds.Domain.Builds.Abstractions;
using PANiXiDA.TacticalHeroes.Builds.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Builds.Infrastructure.Persistence.Features.Builds.Write;

public sealed class BuildsRepository(BuildsWriteDbContext dbContext)
    :
        EfRepository<BuildsWriteDbContext, BuildId, Build>(dbContext),
        IBuildsRepository;
