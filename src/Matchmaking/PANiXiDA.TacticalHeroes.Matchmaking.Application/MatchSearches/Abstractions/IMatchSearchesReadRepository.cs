using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Watch;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Abstractions;

public interface IMatchSearchesReadRepository : IReadRepository<Guid>
{
    Task<MatchSearchStateReadModel?> GetStateByPlayerIdAsync(Guid playerId, CancellationToken cancellationToken);
}
