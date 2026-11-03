using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Matchmaking.Application.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Streaming;

internal sealed class MatchSearchStatePublisher(MatchmakingWriteDbContext context) : IMatchSearchStatePublisher
{
    internal const string Channel = "matchmaking_match_search_state_changed";

    public async Task PublishAsync(Guid playerId, CancellationToken cancellationToken)
    {
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_notify({Channel}, {playerId.ToString()})", cancellationToken);
    }
}
