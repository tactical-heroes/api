using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Process;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Abstractions;

public interface IMatchSearchesReadRepository : IReadRepository<Guid>
{
    Task<IReadOnlyCollection<SearchingMatchSearchReadModel>> GetSearchingAsync(
        DateTimeOffset now,
        CancellationToken cancellationToken);
}
