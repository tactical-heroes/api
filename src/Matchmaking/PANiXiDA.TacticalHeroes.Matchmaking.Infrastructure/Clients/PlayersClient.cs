using PANiXiDA.TacticalHeroes.Matchmaking.Application.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.Players;
using PANiXiDA.TacticalHeroes.Players.Contracts.Players;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Clients;

public sealed class PlayersClient(IPlayersApi playersApi) : IPlayersClient
{
    public async Task<Result<MatchmakingPlayer>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var player = await playersApi.GetByUserIdAsync(userId, cancellationToken);

        return player is null
            ? Result.Failure<MatchmakingPlayer>(Error.NotFound("Player was not found."))
            : Result.Success(new MatchmakingPlayer(player.Id, player.Rating));
    }
}
