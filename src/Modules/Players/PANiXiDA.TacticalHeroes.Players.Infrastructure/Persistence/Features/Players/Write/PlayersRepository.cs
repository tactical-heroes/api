using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Modules.Players.Domain.Players;
using PANiXiDA.TacticalHeroes.Modules.Players.Domain.Players.Abstractions;
using PANiXiDA.TacticalHeroes.Modules.Players.Domain.Users;
using PANiXiDA.TacticalHeroes.Modules.Players.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Modules.Players.Infrastructure.Persistence.Features.Players.Write;

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
