using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Players.Application.Players.Abstractions;
using PANiXiDA.TacticalHeroes.Players.Application.Players.GetProfileByUserId;
using PANiXiDA.TacticalHeroes.Players.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Players.Infrastructure.Persistence.Features.Players.Read.DbModels;
using PANiXiDA.TacticalHeroes.Players.Infrastructure.Persistence.Features.Players.Read.GetProfileByUserId;

namespace PANiXiDA.TacticalHeroes.Players.Infrastructure.Persistence.Features.Players.Read;

public sealed class PlayersReadRepository(PlayersReadDbContext dbContext)
    :
        EfReadRepository<PlayersReadDbContext, Guid, PlayerReadDbModel>(dbContext),
        IPlayersReadRepository
{
    public Task<PlayerProfileReadModel?> GetProfileByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var query = Query.Where(player => player.UserId == userId && player.DeletedAt == null);

        return PlayerProfileReadModelMapper.ProjectTo(query).SingleOrDefaultAsync(cancellationToken);
    }
}
