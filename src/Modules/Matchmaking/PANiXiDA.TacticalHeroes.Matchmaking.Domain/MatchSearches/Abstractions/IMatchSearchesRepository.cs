using PANiXiDA.TacticalHeroes.Modules.Matchmaking.Domain.Players;

namespace PANiXiDA.TacticalHeroes.Modules.Matchmaking.Domain.MatchSearches.Abstractions;

public interface IMatchSearchesRepository : IRepository<MatchSearchId, MatchSearch>
{
    Task<List<MatchSearch>> GetAllAsync(
        ISpecification<MatchSearch> specification,
        CancellationToken cancellationToken);

    Task<MatchSearch?> GetActiveByPlayerIdAsync(PlayerId playerId, CancellationToken cancellationToken);
}
