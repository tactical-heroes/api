using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Players;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Abstractions;

public interface IMatchSearchesRepository : IRepository<MatchSearchId, MatchSearch>
{
    Task<List<MatchSearch>> GetAllAsync(
        ISpecification<MatchSearch> specification,
        CancellationToken cancellationToken);

    Task<MatchSearch?> GetActiveByPlayerIdAsync(PlayerId playerId, CancellationToken cancellationToken);
}
