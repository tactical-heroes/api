using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchSearches.Write;

public sealed class MatchSearchesRepository(
    MatchmakingWriteDbContext dbContext)
    :
        EfRepository<MatchmakingWriteDbContext, MatchSearchId, MatchSearch>(dbContext),
        IMatchSearchesRepository;
