using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Common.ReadModels;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Abstractions;

public interface IBuildsClient
{
    Task<BuildReadModel?> GetByIdAsync(Guid buildId, CancellationToken cancellationToken);
}
