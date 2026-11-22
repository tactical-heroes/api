using PANiXiDA.TacticalHeroes.Matchmaking.Application.ReadModels;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.Abstractions;

public interface IBuildsClient
{
    Task<BuildReadModel?> GetByIdAsync(Guid buildId, CancellationToken cancellationToken);
}
