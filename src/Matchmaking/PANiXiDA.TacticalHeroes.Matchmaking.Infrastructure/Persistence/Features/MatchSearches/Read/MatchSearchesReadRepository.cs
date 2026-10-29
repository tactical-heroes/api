using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Process;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchSearches.Read.DbModels;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchSearches.Read.Process;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchSearches.Read;

public sealed class MatchSearchesReadRepository(MatchmakingReadDbContext dbContext)
    :
        EfReadRepository<MatchmakingReadDbContext, Guid, MatchSearchReadDbModel>(dbContext),
        IMatchSearchesReadRepository
{
    public async Task<IReadOnlyCollection<SearchingMatchSearchReadModel>> GetSearchingAsync(
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var query = Query.Where(
            search => search.Status == MatchSearchStatus.Searching.Name
                && search.QueuedAt <= now
                && search.ExpiresAt > now);
        var projected = SearchingMatchSearchReadModelMapper.ProjectTo(query);
        return await SearchingMatchSearchReadModelSorting.ApplySorting(projected, SortingParameters.None)
            .ToListAsync(cancellationToken);
    }
}
