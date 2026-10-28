using PANiXiDA.TacticalHeroes.Matchmaking.Application.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.ReadModels;
using PANiXiDA.TacticalHeroes.Players.Contracts.Players;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Clients;

public sealed class PlayersClient(IPlayersApi playersApi) : IPlayersClient
{
    public async Task<Result<MatchmakingPlayerReadModel>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var player = await playersApi.GetByUserIdAsync(userId, cancellationToken);

        return player is null
            ? Result.Failure<MatchmakingPlayerReadModel>(Error.NotFound("Player was not found."))
            : Result.Success(new MatchmakingPlayerReadModel(player.Id, player.Rating));
    }
}
