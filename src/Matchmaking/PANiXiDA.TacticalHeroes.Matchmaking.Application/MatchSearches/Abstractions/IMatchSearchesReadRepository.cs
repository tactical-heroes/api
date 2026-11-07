using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Watch;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Abstractions;

public interface IMatchSearchesReadRepository : IReadRepository<Guid>
{
    Task<MatchSearchReadModel?> GetStateByPlayerIdAsync(Guid playerId, CancellationToken cancellationToken);
}
