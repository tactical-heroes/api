using PANiXiDA.TacticalHeroes.Players.Application.Players.Abstractions;
using PANiXiDA.TacticalHeroes.Players.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Players.Infrastructure.Persistence.Features.Players.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Players.Infrastructure.Persistence.Features.Players.Read;

public sealed class PlayersReadRepository(PlayersReadDbContext dbContext)
    :
        EfReadRepository<PlayersReadDbContext, Guid, PlayerReadDbModel>(dbContext),
        IPlayersReadRepository;
