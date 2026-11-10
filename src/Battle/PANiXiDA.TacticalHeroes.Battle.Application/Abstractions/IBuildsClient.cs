using PANiXiDA.TacticalHeroes.Battle.Application.ReadModels;

namespace PANiXiDA.TacticalHeroes.Battle.Application.Abstractions;

public interface IBuildsClient
{
    Task<BuildReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
