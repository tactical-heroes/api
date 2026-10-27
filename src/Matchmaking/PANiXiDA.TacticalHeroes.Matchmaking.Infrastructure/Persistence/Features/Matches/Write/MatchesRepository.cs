using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Matches;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Matches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.Matches.Write;

public sealed class MatchesRepository(
    MatchmakingWriteDbContext dbContext)
    :
        EfRepository<MatchmakingWriteDbContext, MatchId, Match>(dbContext),
        IMatchesRepository;
