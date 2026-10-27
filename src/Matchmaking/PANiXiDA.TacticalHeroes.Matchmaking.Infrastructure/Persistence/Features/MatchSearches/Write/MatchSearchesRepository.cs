using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Players;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchSearches.Write;

public sealed class MatchSearchesRepository(
    MatchmakingWriteDbContext dbContext)
    :
        EfRepository<MatchmakingWriteDbContext, MatchSearchId, MatchSearch>(dbContext),
        IMatchSearchesRepository
{
    private readonly MatchmakingWriteDbContext _dbContext = dbContext;

    public async Task<MatchSearch?> GetActiveByPlayerIdForUpdateAsync(
        PlayerId playerId,
        CancellationToken cancellationToken)
    {
        if (_dbContext.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("An active transaction is required to lock a player's search.");
        }

        var lockKey = $"matchmaking:player:{playerId.Value}";
        await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock(hashtextextended({lockKey}, 0))", cancellationToken);

        return await _dbContext.Set<MatchSearch>()
            .FromSqlInterpolated(
                $"""
                SELECT match_searches.*, match_searches.xmin FROM matchmaking.match_searches
                WHERE player_id = {playerId.Value}
                  AND deleted_at IS NULL
                  AND status IN ({MatchSearchStatus.Searching.Name}, {MatchSearchStatus.Matched.Name})
                FOR UPDATE
                """)
            .SingleOrDefaultAsync(cancellationToken);
    }
}
