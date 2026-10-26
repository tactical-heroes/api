using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchAssignments.Write;

public sealed class MatchAssignmentsRepository(MatchmakingWriteDbContext dbContext)
    :
        EfRepository<MatchmakingWriteDbContext, MatchAssignmentId, MatchAssignment>(dbContext),
        IMatchAssignmentsRepository;
