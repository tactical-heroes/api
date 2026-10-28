using PANiXiDA.TacticalHeroes.Players.Contracts.Players;

namespace PANiXiDA.TacticalHeroes.Players.Presentation.Internal;

internal sealed class PlayersApi(IMediator mediator) : IPlayersApi
{
    public async Task<Result<PlayerResponse>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var result = await mediator.QueryAsync(PlayersApiMapper.ToQuery(userId), cancellationToken);

        return result.Map(PlayersApiMapper.ToResponse);
    }
}
