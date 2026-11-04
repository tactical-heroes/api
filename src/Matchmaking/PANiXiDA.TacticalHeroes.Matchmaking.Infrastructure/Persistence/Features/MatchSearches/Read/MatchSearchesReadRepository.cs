using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Watch;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchSearches.Read.DbModels;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchSearches.Read.Watch;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchSearches.Read;

public sealed class MatchSearchesReadRepository(MatchmakingReadDbContext dbContext)
    :
        EfReadRepository<MatchmakingReadDbContext, Guid, MatchSearchReadDbModel>(dbContext),
        IMatchSearchesReadRepository
{
    public Task<MatchSearchStateReadModel?> GetStateByPlayerIdAsync(
        Guid playerId,
        CancellationToken cancellationToken)
    {
        var query = Query
            .AsNoTracking()
            .Where(search => search.PlayerId == playerId)
            .OrderByDescending(search => search.Status == MatchSearchStatus.Searching.Name
                || search.Status == MatchSearchStatus.Matched.Name)
            .ThenByDescending(search => search.CreatedAt)
            .ThenByDescending(search => search.Id);

        return MatchSearchStateReadModelMapper.ProjectTo(query).FirstOrDefaultAsync(cancellationToken);
    }
}
