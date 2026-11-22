using PANiXiDA.TacticalHeroes.Modules.Games.Application.Games.Abstractions;
using PANiXiDA.TacticalHeroes.Modules.Games.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Modules.Games.Infrastructure.Persistence.Features.Games.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Modules.Games.Infrastructure.Persistence.Features.Games.Read;

public sealed class GamesReadRepository(GamesReadDbContext dbContext)
    :
        EfReadRepository<GamesReadDbContext, Guid, GameReadDbModel>(dbContext),
        IGamesReadRepository;
