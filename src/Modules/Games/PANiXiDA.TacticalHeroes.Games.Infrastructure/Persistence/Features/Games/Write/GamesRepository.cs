using PANiXiDA.TacticalHeroes.Modules.Games.Domain.Games;
using PANiXiDA.TacticalHeroes.Modules.Games.Domain.Games.Abstractions;
using PANiXiDA.TacticalHeroes.Modules.Games.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Modules.Games.Infrastructure.Persistence.Features.Games.Write;

public sealed class GamesRepository(GamesWriteDbContext dbContext)
    :
        EfRepository<GamesWriteDbContext, GameId, Game>(dbContext),
        IGamesRepository;
