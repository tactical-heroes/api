using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Modules.Matchmaking.Application.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Modules.Matchmaking.Application.MatchSearches.Common;
using PANiXiDA.TacticalHeroes.Modules.Matchmaking.Domain.MatchSearches.Enumerations;
using PANiXiDA.TacticalHeroes.Modules.Matchmaking.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Modules.Matchmaking.Infrastructure.Persistence.Features.MatchSearches.Read.Common;
using PANiXiDA.TacticalHeroes.Modules.Matchmaking.Infrastructure.Persistence.Features.MatchSearches.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Modules.Matchmaking.Infrastructure.Persistence.Features.MatchSearches.Read;

public sealed class MatchSearchesReadRepository(MatchmakingReadDbContext dbContext)
    :
        EfReadRepository<MatchmakingReadDbContext, Guid, MatchSearchReadDbModel>(dbContext),
        IMatchSearchesReadRepository
{
    public Task<MatchSearchReadModel?> GetStateByPlayerIdAsync(
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

        return MatchSearchReadModelMapper.ProjectTo(query).FirstOrDefaultAsync(cancellationToken);
    }
}
