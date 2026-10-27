using PANiXiDA.TacticalHeroes.Players.Domain.Players;
using PANiXiDA.TacticalHeroes.Players.Domain.Players.Abstractions;
using PANiXiDA.TacticalHeroes.Players.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Players.Infrastructure.Persistence.Features.Players.Write;

public sealed class PlayersRepository(PlayersWriteDbContext dbContext)
    :
        EfRepository<PlayersWriteDbContext, PlayerId, Player>(dbContext),
        IPlayersRepository;
