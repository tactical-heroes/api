using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Common;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Abstractions;

public interface IMatchSearchesReadRepository : IReadRepository<Guid>
{
    Task<MatchSearchReadModel?> GetStateByPlayerIdAsync(Guid playerId, CancellationToken cancellationToken);
}
