using Microsoft.EntityFrameworkCore;

using PANiXiDA.Core.SpecificationPattern.Abstractions;
using PANiXiDA.Core.SpecificationPattern.Extensions;

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
    public async Task<IReadOnlyCollection<MatchSearch>> GetAllAsync(
        ISpecification<MatchSearch> specification,
        CancellationToken cancellationToken)
    {
        return await Query
            .Where(specification)
            .OrderBy(search => search.QueuedAt)
            .ThenBy(search => search.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<MatchSearch?> GetActiveByPlayerIdAsync(
        PlayerId playerId,
        CancellationToken cancellationToken)
    {
        return Query
            .Where(search => search.PlayerId == playerId
                && (search.Status == MatchSearchStatus.Searching || search.Status == MatchSearchStatus.Matched))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
