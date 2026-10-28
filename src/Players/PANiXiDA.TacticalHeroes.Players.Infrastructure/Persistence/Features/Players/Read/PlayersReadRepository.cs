using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Players.Application.Players.Abstractions;
using PANiXiDA.TacticalHeroes.Players.Application.Players.GetInternalPlayer;
using PANiXiDA.TacticalHeroes.Players.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Players.Infrastructure.Persistence.Features.Players.Read.DbModels;
using PANiXiDA.TacticalHeroes.Players.Infrastructure.Persistence.Features.Players.Read.GetInternalPlayer;

namespace PANiXiDA.TacticalHeroes.Players.Infrastructure.Persistence.Features.Players.Read;

public sealed class PlayersReadRepository(PlayersReadDbContext dbContext)
    :
        EfReadRepository<PlayersReadDbContext, Guid, PlayerReadDbModel>(dbContext),
        IPlayersReadRepository
{
    public Task<PlayerReadModel?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var query = Query.Where(player => player.UserId == userId && player.DeletedAt == null);

        return PlayerReadModelMapper.ProjectTo(query).SingleOrDefaultAsync(cancellationToken);
    }
}
