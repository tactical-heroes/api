using PANiXiDA.TacticalHeroes.Players.Application.Players.Abstractions;

namespace PANiXiDA.TacticalHeroes.Players.Application.Players.GetInternalPlayer;

public sealed class GetInternalPlayerHandler(IPlayersReadRepository playersReadRepository)
    : IQueryHandler<GetInternalPlayerQuery, Result<PlayerReadModel>>
{
    public async Task<Result<PlayerReadModel>> HandleAsync(
        GetInternalPlayerQuery query,
        CancellationToken cancellationToken)
    {
        var player = await playersReadRepository.GetByUserIdAsync(query.UserId, cancellationToken);

        return player is null
            ? Result.Failure<PlayerReadModel>(Error.NotFound("Player was not found."))
            : Result.Success(player);
    }
}
