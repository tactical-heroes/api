using PANiXiDA.TacticalHeroes.Games.Domain.Games;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Abstractions;
using PANiXiDA.TacticalHeroes.Games.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Games.Infrastructure.Persistence.Features.Games.Write;

public sealed class GamesRepository(GamesWriteDbContext dbContext)
    :
        EfRepository<GamesWriteDbContext, GameId, Game>(dbContext),
        IGamesRepository;
