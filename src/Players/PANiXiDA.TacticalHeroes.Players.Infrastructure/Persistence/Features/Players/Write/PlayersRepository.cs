using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Players.Domain.Players;
using PANiXiDA.TacticalHeroes.Players.Domain.Players.Abstractions;
using PANiXiDA.TacticalHeroes.Players.Domain.Users;
using PANiXiDA.TacticalHeroes.Players.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Players.Infrastructure.Persistence.Features.Players.Write;

public sealed class PlayersRepository(PlayersWriteDbContext dbContext)
    :
        EfRepository<PlayersWriteDbContext, PlayerId, Player>(dbContext),
        IPlayersRepository
{
    public Task<Player?> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken)
    {
        return Query.FirstOrDefaultAsync(player => player.UserId == userId, cancellationToken);
    }
}
