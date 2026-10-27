using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Players;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Abstractions;

public interface IMatchSearchesRepository : IRepository<MatchSearchId, MatchSearch>
{
    Task<MatchSearch?> GetActiveByPlayerIdForUpdateAsync(PlayerId playerId, CancellationToken cancellationToken);
}
