using PANiXiDA.TacticalHeroes.Players.Contracts.Players;

namespace PANiXiDA.TacticalHeroes.Players.Presentation.Internal;

internal sealed class PlayersApi(IMediator mediator) : IPlayersApi
{
    public async Task<PlayerResponse?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var result = await mediator.QueryAsync(PlayersApiMapper.ToQuery(userId), cancellationToken);

        return result.IsSuccess ? PlayersApiMapper.ToResponse(result.Value) : null;
    }
}
