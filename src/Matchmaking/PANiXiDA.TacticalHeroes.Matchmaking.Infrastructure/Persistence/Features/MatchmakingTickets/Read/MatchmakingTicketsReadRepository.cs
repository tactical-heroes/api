using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchmakingTickets.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchmakingTickets.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchmakingTickets.Read;

public sealed class MatchmakingTicketsReadRepository(MatchmakingReadDbContext dbContext)
    :
        EfReadRepository<MatchmakingReadDbContext, Guid, MatchmakingTicketReadDbModel>(dbContext),
        IMatchmakingTicketsReadRepository;
