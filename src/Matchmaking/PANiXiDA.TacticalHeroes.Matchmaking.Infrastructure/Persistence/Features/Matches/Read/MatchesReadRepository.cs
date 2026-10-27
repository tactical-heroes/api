using PANiXiDA.TacticalHeroes.Matchmaking.Application.Matches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.Matches.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.Matches.Read;

public sealed class MatchesReadRepository(MatchmakingReadDbContext dbContext)
    :
        EfReadRepository<MatchmakingReadDbContext, Guid, MatchReadDbModel>(dbContext),
        IMatchesReadRepository;
