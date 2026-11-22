using PANiXiDA.TacticalHeroes.Modules.Builds.Domain.Builds;
using PANiXiDA.TacticalHeroes.Modules.Builds.Domain.Builds.Abstractions;
using PANiXiDA.TacticalHeroes.Modules.Builds.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Modules.Builds.Infrastructure.Persistence.Features.Builds.Write;

public sealed class BuildsRepository(BuildsWriteDbContext dbContext)
    :
        EfRepository<BuildsWriteDbContext, BuildId, Build>(dbContext),
        IBuildsRepository;
