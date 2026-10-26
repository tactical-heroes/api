using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchAssignments.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchAssignments.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchAssignments.Read;

public sealed class MatchAssignmentsReadRepository(MatchmakingReadDbContext dbContext)
    :
        EfReadRepository<MatchmakingReadDbContext, Guid, MatchAssignmentReadDbModel>(dbContext),
        IMatchAssignmentsReadRepository;
