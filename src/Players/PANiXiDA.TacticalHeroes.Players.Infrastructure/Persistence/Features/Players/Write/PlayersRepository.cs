using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Players.Domain.Players;
using PANiXiDA.TacticalHeroes.Players.Domain.Players.Abstractions;
using PANiXiDA.TacticalHeroes.Players.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Players.Infrastructure.Persistence.Features.Players.Write;

public sealed class PlayersRepository(PlayersWriteDbContext dbContext, TimeProvider timeProvider)
    :
        EfRepository<PlayersWriteDbContext, PlayerId, Player>(dbContext),
        IPlayersRepository
{
    private readonly PlayersWriteDbContext _dbContext = dbContext;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task AddIfMissingAsync(Player player, CancellationToken cancellationToken)
    {
        var nowUtc = _timeProvider.GetUtcNow().UtcDateTime;
        var avatarFileId = player.AvatarFileId?.Value;

        await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO players.players (id, user_id, avatar_file_id, rating, created_at, updated_at)
            VALUES ({player.Id.Value}, {player.UserId.Value}, {avatarFileId}, {player.Rating.Value}, {nowUtc}, {nowUtc})
            ON CONFLICT (user_id) DO NOTHING
            """,
            cancellationToken);
    }
}
