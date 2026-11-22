using PANiXiDA.TacticalHeroes.Modules.Matchmaking.Application.ReadModels;

namespace PANiXiDA.TacticalHeroes.Modules.Matchmaking.Application.Abstractions;

public interface IBuildsClient
{
    Task<BuildReadModel?> GetByIdAsync(Guid buildId, CancellationToken cancellationToken);
}
