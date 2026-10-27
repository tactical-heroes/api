using PANiXiDA.TacticalHeroes.Games.Application.Games.Abstractions;
using PANiXiDA.TacticalHeroes.Games.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Games.Infrastructure.Persistence.Features.Games.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Games.Infrastructure.Persistence.Features.Games.Read;

public sealed class GamesReadRepository(GamesReadDbContext dbContext)
    :
        EfReadRepository<GamesReadDbContext, Guid, GameReadDbModel>(dbContext),
        IGamesReadRepository;
