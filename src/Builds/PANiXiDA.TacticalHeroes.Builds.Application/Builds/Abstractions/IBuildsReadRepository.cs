using PANiXiDA.TacticalHeroes.Builds.Application.Builds.GetInternalBuild;

namespace PANiXiDA.TacticalHeroes.Builds.Application.Builds.Abstractions;

public interface IBuildsReadRepository : IReadRepository<Guid>
{
    Task<BuildReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
